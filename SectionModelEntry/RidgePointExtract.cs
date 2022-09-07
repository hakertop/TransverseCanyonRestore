using GdalLib;
using GeoCommon;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanyonRecoveryEntry
{
    public class RidgePointExtract
    {
        /// <summary>
        /// 等值线字典
        /// </summary>
        private Dictionary<int,Geometry> _contour= new Dictionary<int, Geometry>();

        /// <summary>
        /// 等值线高程字典
        /// </summary>
        private Dictionary<int, double> _contourValue = new Dictionary<int, double>();

        /// <summary>
        /// 等值线是否闭合字典
        /// </summary>
        private Dictionary<int, bool> _contourClose = new Dictionary<int, bool>();

        private DEMRaster _raster = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="contourFiles"></param>
        public RidgePointExtract(string contourFiles,string pDEMFile)
        {
            _contour = GetContourLines(contourFiles,ref _contourValue, ref _contourClose);

            _raster = new DEMRaster(pDEMFile);
        }


        /// <summary>
        /// 创建特征点
        /// </summary>
        public void CreateCharacteristicPoints()
        {
            List<Vertex2D> pCharacteristicPoints = new List<Vertex2D>();

            foreach (var vid in _contour.Keys)
            {
                //获取一个等值线
                Geometry pContour = _contour[vid];
                if(_contourClose[vid])
                {
                    //最左边的点的Id          
                    int leftestId = -1;
                    GetLeftestPoint(pContour,ref leftestId);

                    //Geometry ptleft = new Geometry(wkbGeometryType.wkbPoint);
                    //ptleft.AddPoint_2D(pContour.GetX(leftestId), pContour.GetY(leftestId));
                    //pCharacteristicPoints.Add(ptleft);

                    Vertex2D ptleft = new Vertex2D(pContour.GetX(leftestId), pContour.GetY(leftestId));
                    pCharacteristicPoints.Add(ptleft);

                    //最右边的点的Id
                    int rightestId = -1;
                    GetRightestPoint(pContour, ref rightestId);

                    //Geometry ptRight = new Geometry(wkbGeometryType.wkbPoint);
                    //ptRight.AddPoint_2D(pContour.GetX(rightestId), pContour.GetY(rightestId));
                    //pCharacteristicPoints.Add(ptRight);

                    Vertex2D ptRight = new Vertex2D(pContour.GetX(rightestId), pContour.GetY(rightestId));
                    pCharacteristicPoints.Add(ptRight);



                    //分割成两个线段
                    List<Geometry> pTwoSegments = SplitContourByTwoId(pContour, leftestId, rightestId);
                   // pTwoSegments.ExportGeometryToShapfile(@"E:\MyEssay\CanyonRecovery\StudyData\wuxia\RidgeLineTest", "TwoLines");

                    //每个线段分别求特征点                   
                    GetCharacteristicPoints(pTwoSegments[0],ref pCharacteristicPoints);
                    GetCharacteristicPoints(pTwoSegments[1], ref pCharacteristicPoints);

                    Console.WriteLine("完成"+vid);
                }
                else
                {
                    GetCharacteristicPoints(pContour, ref pCharacteristicPoints);
                    Console.WriteLine("完成" + vid);
                }
            }


            //分成三类输出
            List<Geometry> pRidgePoints = new List<Geometry>();
            List<Geometry> pValleyPoints = new List<Geometry>();
            List<Geometry> pUnKnowPoints = new List<Geometry>();

            foreach (var vd in pCharacteristicPoints)
            {
                Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
                pt.AddPoint_2D(vd.x, vd.y);
                if (vd.id==0)
                {                
                    pRidgePoints.Add(pt);
                }
                else if(vd.id==1)
                {                 
                    pValleyPoints.Add(pt);
                }
                else
                {
                    pUnKnowPoints.Add(pt);
                }
            }
            pRidgePoints.TrimExcess();
            pValleyPoints.TrimExcess();
            pUnKnowPoints.TrimExcess();

            pRidgePoints.ExportGeometryToShapfile(@"E:\MyEssay\CanyonRecovery\StudyData\wuxia\RidgeLineTest", "pRidgePoints");
            pValleyPoints.ExportGeometryToShapfile(@"E:\MyEssay\CanyonRecovery\StudyData\wuxia\RidgeLineTest", "pValleyPoints");
           
            pUnKnowPoints.ExportGeometryToShapfile(@"E:\MyEssay\CanyonRecovery\StudyData\wuxia\RidgeLineTest", "pUnKnowPoints");

            double ds = 0;
        }


        #region 针对山脊线的提取

        /// <summary>
        /// 从shp种提取出山脊线集合
        /// </summary>
        /// <param name="pPath">shp文件路径</param>
        /// <param name="pHeightValue">保存高程值</param>
        /// <param name="pContourClose">是否闭合</param>
        /// <returns></returns>
        public  Dictionary<int, Geometry> GetContourLines(string pPath,ref Dictionary<int,double> pContourValue
            ,ref Dictionary<int, bool> pContourClose)
        {
            Dictionary<int, Geometry> pGeometry = new Dictionary<int, Geometry>();

            Layer pContourLayer = LayerHelp.GetLayerByLayerName(pPath);
            int PFeatureCount = (int)pContourLayer.GetFeatureCount(0);
            for (int i = 0; i < PFeatureCount; i++)
            {
                int id = pContourLayer.GetFeature(i).GetFieldAsInteger("Id");
                double value = pContourLayer.GetFeature(i).GetFieldAsDouble("Contour");
                Geometry pg = pContourLayer.GetFeature(i).GetGeometryRef();

                //判断是单一的Line还是多段线
                wkbGeometryType PT = pg.GetGeometryType();
                if (PT == wkbGeometryType.wkbLineString)
                {
                    if (pg.GetX(0) == pg.GetX(pg.GetPointCount() - 1) && pg.GetY(0) == pg.GetY(pg.GetPointCount() - 1))
                        pContourClose.Add(id,true);
                    else
                        pContourClose.Add(id, false);


                    pGeometry.Add(id, pg);

                }
                else if(PT == wkbGeometryType.wkbMultiLineString)
                {
                    if (pg.GetX(0) == pg.GetX(pg.GetPointCount() - 1) && pg.GetY(0) == pg.GetY(pg.GetPointCount() - 1))
                        pContourClose.Add(id, true);
                    else
                        pContourClose.Add(id, false);


                    pGeometry.Add(id, GeometrySolve.ConverteMultiLineToSingleLine(pg));
                }

                pContourValue.Add(id, value);
            }
            return pGeometry;
        }

        #region 获取四个方向最远点的序号
        /// <summary>
        /// 获取最左边的点序号
        /// </summary>
        /// <param name="pContourLines"></param>
        public void GetLeftestPoint(Geometry pContour,ref int pLeftestPointId)
        {

            double x = pContour.GetX(0);

            for(int i=0;i< pContour.GetPointCount();i++)
            {
                if(x>= pContour.GetX(i))
                {
                    pLeftestPointId = i;

                    x = pContour.GetX(i);
                }
            }
        }


        /// <summary>
        /// 获取最下边的点序号
        /// </summary>
        /// <param name="pContourLines"></param>
        public void GetLowestPoint(Geometry pContour, ref int pLeftestPointId)
        {

            double y = pContour.GetY(0);

            for (int i = 0; i < pContour.GetPointCount(); i++)
            {
                if (y >= pContour.GetY(i))
                {
                    pLeftestPointId = i;

                    y = pContour.GetX(i);
                }
            }
        }

        /// <summary>
        /// 获取最右边的点的序号
        /// </summary>
        /// <param name="pContourLines"></param>
        public void GetRightestPoint(Geometry pContour, ref int pRightestPointId)
        {

            double x = pContour.GetX(0);

            for (int i = 0; i < pContour.GetPointCount(); i++)
            {
                if ( pContour.GetX(i)>=x)
                {
                    pRightestPointId = i;

                    x = pContour.GetX(i);
                }
            }
        }


        /// <summary>
        /// 获取最上边的点的序号
        /// </summary>
        /// <param name="pContourLines"></param>
        public void GetHightestPoint(Geometry pContour, ref int pRightestPointId)
        {

            double y = pContour.GetY(0);

            for (int i = 0; i < pContour.GetPointCount(); i++)
            {
                if (pContour.GetY(i) >= y)
                {
                    pRightestPointId = i;

                    y = pContour.GetX(i);
                }
            }
        }
        #endregion


        #region 点切割线函数 一种是两个点切割一条闭合线 一种是一个点切割一条非闭合线

        /// <summary>
        /// 根据两个点切割等值线 若是闭合线，会分割成两条线
        /// </summary>
        /// <param name="pContourLines"></param>
        public List<Geometry> SplitContourByTwoId(Geometry pContour, int _leftestId,int _rightestId)
        {
            List<Geometry> pSegments = new List<Geometry>();

            Geometry pOneSegment = new Geometry(wkbGeometryType.wkbLineString);

            Geometry pAnotherSegment = new Geometry(wkbGeometryType.wkbLineString);

            if (_leftestId<= _rightestId)
            {
                for(int i= _leftestId;i<(_rightestId+1);i++)
                {
                    pOneSegment.AddPoint_2D(pContour.GetX(i), pContour.GetY(i));

                }

                for(int i= _rightestId;i< (pContour.GetPointCount()-1);i++)
                {
                    pAnotherSegment.AddPoint_2D(pContour.GetX(i), pContour.GetY(i));
                }

                for (int i = 0; i <= _leftestId; i++)
                {
                    pAnotherSegment.AddPoint_2D(pContour.GetX(i), pContour.GetY(i));
                }

                pSegments.Add(pOneSegment);
                pSegments.Add(pAnotherSegment);
            }

            else
            {
                for (int i = _rightestId; i < (_leftestId + 1); i++)
                {
                    pOneSegment.AddPoint_2D(pContour.GetX(i), pContour.GetY(i));

                }

                for (int i = _leftestId; i < (pContour.GetPointCount() - 1); i++)
                {
                    pAnotherSegment.AddPoint_2D(pContour.GetX(i), pContour.GetY(i));
                }

                for (int i = 0; i <= _rightestId; i++)
                {
                    pAnotherSegment.AddPoint_2D(pContour.GetX(i), pContour.GetY(i));
                }

                pSegments.Add(pOneSegment);
                pSegments.Add(pAnotherSegment);
            }

            return pSegments;
        }


        /// <summary>
        /// 非闭合线段，根据1个点切割,一定获取到两条线
        /// </summary>
        /// <param name="pContourLines"></param>
        /// <param name="_oneId">切割位置的ID</param>
        public Geometry[] SplitContourByOneId(Geometry pContour, int _oneId)
        {
            Geometry[] pSegments = new Geometry[2];

            Geometry pOneSegment = new Geometry(wkbGeometryType.wkbLineString);

            Geometry pAnotherSegment = new Geometry(wkbGeometryType.wkbLineString);

            for (int i = 0; i <= _oneId; i++)
            {
                pOneSegment.AddPoint_2D(pContour.GetX(i), pContour.GetY(i));

            }

            for (int i = _oneId; i < pContour.GetPointCount(); i++)
            {
                pAnotherSegment.AddPoint_2D(pContour.GetX(i), pContour.GetY(i));
            }

            pSegments[0]=pOneSegment;
            pSegments[1]=pAnotherSegment;

            return pSegments;
        }

        #endregion


        #region 确定特征点的函数

        /// <summary>
        /// 求出所有的特征点(递归的方法)
        /// </summary>
        /// <param name="psegment"></param>
        /// <param name="_allCharacteristicPoints"></param>
        public void GetCharacteristicPoints(Geometry psegment, ref List<Vertex2D> _allCharacteristicPoints)
        {
            if (psegment.GetPointCount() > 2)
            {

                //最大垂距点
                Dictionary<int, Geometry> oneMaxVerticalPoint = GetMaxVerticalPoint(psegment, GetVerticalPoints(psegment));
                //确定是否是特征点
                bool ischara = IsCharacteristicPoint(psegment, oneMaxVerticalPoint, 150);

                //是特征点就保存下来
                if (ischara)
                {
                    //确定是山脊点还是山谷点
                    Geometry pCharaP = oneMaxVerticalPoint.Values.ToList<Geometry>()[0];
                    Vertex2D pCharaPoint = new Vertex2D(pCharaP.GetX(0), pCharaP.GetY(0));
                    int ptFlag = RidgeOrValleyPoint(psegment, oneMaxVerticalPoint, _raster);
                    pCharaPoint.id = ptFlag;
                    _allCharacteristicPoints.Add(pCharaPoint);

                    //一定切割成两个
                    Geometry[] pSegments = SplitContourByOneId(psegment, oneMaxVerticalPoint.Keys.ToList<int>()[0]);

                    //两个弧段继续求特征点
                    GetCharacteristicPoints(pSegments[0], ref _allCharacteristicPoints);
                    GetCharacteristicPoints(pSegments[1], ref _allCharacteristicPoints);
                }
            }
        }

        /// <summary>
        /// 确定特征点的类型
        /// </summary>
        /// <param name="_pSegment"></param>
        /// <param name="_pVerticalPoints"></param>
        /// <param name="_raster"></param>
        /// <returns></returns>
        public int  RidgeOrValleyPoint(Geometry _pSegment, Dictionary<int, Geometry> _pVerticalPoints, DEMRaster _raster)
        {

            int id = _pVerticalPoints.Keys.ToList<int>()[0];
       
            Geometry pt2 = new Geometry(wkbGeometryType.wkbPoint);
            pt2.AddPoint_2D(_pSegment.GetX(id-1), _pSegment.GetY(id-1));

            Geometry pt3 = new Geometry(wkbGeometryType.wkbPoint);
            pt3.AddPoint_2D(_pSegment.GetX(id+1), _pSegment.GetY(id+1));


            Geometry pt1 = new Geometry(wkbGeometryType.wkbPoint);
            pt1.AddPoint_2D(_pSegment.GetX(id), _pSegment.GetY(id));

            //分辨率
            double pixelValue = _raster.GetPixel();


            //垂足点
            Geometry pft = GeometrySolve.GetFootPoint(pt1, pt2, pt3);

            List<double> pPointCol = new List<double>();
            pPointCol.Add(_raster.GetElevation(pt1.GetX(0), pt1.GetY(0)));

            Geometry ptExtend = new Geometry(wkbGeometryType.wkbPoint);
            ptExtend = pft;
            pPointCol.Add(_raster.GetElevation(pft.GetX(0), pft.GetY(0)));
            //按照斜率延长3-5个点
            for (int i=0;i<1;i++)
            {
                double[] COORD = GetExtenLinePointBaseDistance(pt1, ptExtend, pixelValue);
                pPointCol.Add(_raster.GetElevation(COORD[0], COORD[1]));
            }

            if (pPointCol[0] <= pPointCol[1] && pPointCol[1] <= pPointCol[2]) //&& pPointCol[2] <= pPointCol[3] && pPointCol[3] <= pPointCol[4])
                return 0;
            //else if (pPointCol[0] <= pPointCol[1] && pPointCol[1] <= pPointCol[2] && pPointCol[2] <= pPointCol[3])
            //    return 0;
            //else if (pPointCol[0] <= pPointCol[1] && pPointCol[1] <= pPointCol[2])
            //    return 0;
            //else if (pPointCol[0] <= pPointCol[1])
            //    return 0;


            else if (pPointCol[0] >= pPointCol[1] && pPointCol[1] >= pPointCol[2]) //&& pPointCol[2] >= pPointCol[3] && pPointCol[3] >= pPointCol[4])
                return 1;
            //else if (pPointCol[0] >= pPointCol[1] && pPointCol[1] >= pPointCol[2] && pPointCol[2] >= pPointCol[3])
            //    return 1;
            //else if (pPointCol[0] >= pPointCol[1] && pPointCol[1] >= pPointCol[2])
            //    return 1;
            //else if (pPointCol[0] >= pPointCol[1])
            //    return 1;
            else
                return -1;
        }


        /// <summary>
        /// 指定延长距离后的确定延长点的坐标
        /// </summary>
        /// <param name="_pStart"></param>
        /// <param name="_pEnd"></param>
        /// <param name="_d"></param>
        /// <returns></returns>
        public double[] GetExtenLinePointBaseDistance(Geometry _pStart,Geometry _pEnd,double _d)
        {
            double[] coord = new double[2];

            double xab, yab;
            double xbd, ybd;
            double xd, yd;

            xab = _pEnd.GetX(0) - _pStart.GetX(0);
            yab = _pEnd.GetY(0) - _pStart.GetY(0);

            if(xab==0.0)
            {
                xd = _pEnd.GetX(0);
                yd = _pEnd.GetY(0) + _d;
            }

            if (xab > 0)
            {
                xbd = Math.Sqrt((_d * _d) / ((yab / xab) * (yab / xab) + 1));
            }
            else
            {
                xbd = -Math.Sqrt((_d * _d) / ((yab / xab) * (yab / xab) + 1));
            }

            xd = _pEnd.GetX(0) + xbd;
            yd = _pEnd.GetY(0) + yab / xab * xbd;


            coord[0] = xd;
            coord[1] = yd;

            return coord;
        }


        /// <summary>
        /// 获取所有垂足点
        /// </summary>
        /// <param name="pSegment"></param>
        /// <returns></returns>
        public Dictionary<int, Geometry> GetVerticalPoints(Geometry pSegment)
        {

            Dictionary<int, Geometry> pVerticalPoints = new Dictionary<int, Geometry>();

            int pointcount = pSegment.GetPointCount();
            //线的起点
            Geometry pt2 = new Geometry(wkbGeometryType.wkbPoint);
            pt2.AddPoint_2D(pSegment.GetX(0), pSegment.GetY(0));

            //线的终点
            Geometry pt3 = new Geometry(wkbGeometryType.wkbPoint);
            pt3.AddPoint_2D(pSegment.GetX(pointcount - 1), pSegment.GetY(pointcount - 1));

            
            for (int i = 1; i < pointcount - 1; i++)
            {
                Geometry pt1 = new Geometry(wkbGeometryType.wkbPoint);
                pt1.AddPoint_2D(pSegment.GetX(i), pSegment.GetY(i));
                //垂足点
                Geometry pft = GeometrySolve.GetFootPoint(pt1, pt2, pt3);
                pVerticalPoints.Add(i, pft);
            }

            return pVerticalPoints;
        }


        /// <summary>
        /// 获取最大垂距点,字典中的Key记录的是点的位置
        /// </summary>
        /// <param name="pSegment"></param>
        /// <returns></returns>
        public Dictionary<int, Geometry> GetMaxVerticalPoint(Geometry pSegment,Dictionary<int, Geometry> _pVerticalPoints)
        {

            Dictionary<int, Geometry> pMaxVerticalPoint = new Dictionary<int, Geometry>();

            double distance = 0.0;
            int pointcount = pSegment.GetPointCount();

            for (int i = 1; i < pointcount - 1; i++)
            {
                Geometry pt1 = new Geometry(wkbGeometryType.wkbPoint);
                pt1.AddPoint_2D(pSegment.GetX(i), pSegment.GetY(i));

                double dt = _pVerticalPoints[i].Distance(pt1);

                if(dt>= distance)
                {
                    pMaxVerticalPoint.Clear();
                    distance = dt;
                    pMaxVerticalPoint.Add(i, pt1);
                }

            }
              return pMaxVerticalPoint;
        }


        /// <summary>
        /// 确定是否是特征点
        /// </summary>
        /// <param name="pSegment"></param>
        /// <returns></returns>
        public bool IsCharacteristicPoint(Geometry pSegment, Dictionary<int, Geometry> _pVerticalPoints,float _angle)
        {     
            int pointcount = pSegment.GetPointCount();

            int id = _pVerticalPoints.Keys.ToList<int>()[0];

            Geometry pt1 = new Geometry(wkbGeometryType.wkbPoint);
            pt1.AddPoint_2D(pSegment.GetX(0), pSegment.GetY(0));

            Geometry pt2 = new Geometry(wkbGeometryType.wkbPoint);
            pt2.AddPoint_2D(pSegment.GetX(pSegment.GetPointCount()-1), pSegment.GetY(pSegment.GetPointCount() - 1));

            float angle =GeometrySolve.GetAngle(_pVerticalPoints[id], pt1, pt2);

            if (angle < _angle)
                return true;
            else
                return false;
        }

        #endregion

        #endregion

    }
}
