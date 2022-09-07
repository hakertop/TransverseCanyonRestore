using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeologicalEntity;
using OSGeo.OGR;
using GdalLib;

namespace CanyonRecoveryEntry
{
    public class CanyonMain
    {
        /// <summary>
        /// 工作空间目录
        /// </summary>
        private string _workspacePath;

        public CanyonMain(string _pathFile)
        {
            _workspacePath = _pathFile;
        }

        /// <summary>
        /// 溯源侵蚀模拟
        /// </summary>
        /// <param name="pLeftControlLine"></param>
        /// <param name="pRightControlLine"></param>
        /// <param name="allpoints"></param>
        /// <param name="pDEMFile"></param>
        /// <returns></returns>
        public List<Geometry>[] Headwarderosion(string pLeftControlLine, string pRightControlLine, List<Geometry> allpoints, string pDEMFile)
        {
            //获取DEM
            DEMRaster _raster = new DEMRaster(pDEMFile);
            //获取DEM的分辨率
            double pixel = _raster.GetPixel();

            Dictionary<int, Geometry> _Left3DControlLine = Create3DLines(pLeftControlLine, pixel, _raster);
            Dictionary<int, Geometry> _Right3DControlLine = Create3DLines(pRightControlLine, pixel, _raster);

            int countLeft = _Left3DControlLine[0].GetPointCount();

            Geometry p1 = new Geometry(wkbGeometryType.wkbPoint);
            p1.AddPoint(_Left3DControlLine[0].GetX(0), _Left3DControlLine[0].GetY(0), _Left3DControlLine[0].GetZ(0));

            
            Geometry p2 = new Geometry(wkbGeometryType.wkbPoint);
            p2.AddPoint(_Left3DControlLine[0].GetX(countLeft-1), _Left3DControlLine[0].GetY(countLeft-1), _Left3DControlLine[0].GetZ(countLeft-1));

            int countRight = _Right3DControlLine[0].GetPointCount();

            Geometry p3 = new Geometry(wkbGeometryType.wkbPoint);
            p3.AddPoint(_Right3DControlLine[0].GetX(0), _Right3DControlLine[0].GetY(0), _Right3DControlLine[0].GetZ(0));


            Geometry p4 = new Geometry(wkbGeometryType.wkbPoint);
            p4.AddPoint(_Right3DControlLine[0].GetX(countRight-1), _Right3DControlLine[0].GetY(countRight-1), _Right3DControlLine[0].GetZ(countRight-1));

            double p12 = p1.Distance(p2);
            double p34 = p3.Distance(p4);

            double D = p12 + p34;

            List<Geometry>[] pGEOcoll = new List<Geometry>[4];

            for(int n=1;n<5;n++)
            {
                List<Geometry> pChangePoints = new List<Geometry>();
                for(int i=0;i< allpoints.Count;i++)
                {
                    double pij = allpoints[i].Distance(p1) + allpoints[i].Distance(p3);

                    double u = ((Math.Sqrt(pij / D) * n) / 4);

                    //原始高程
                    double oz = _raster.GetElevation(allpoints[i].GetX(0), allpoints[i].GetY(0));

                    //现在高程
                    double cz = u * oz + (1 - u) * allpoints[i].GetZ(0);



                    Geometry gp = new Geometry(wkbGeometryType.wkbPoint);

                    if (cz < oz)
                        cz = oz;

                    gp.AddPoint(allpoints[i].GetX(0), allpoints[i].GetY(0), cz);

                    pChangePoints.Add(gp);
                }
                pChangePoints.TrimExcess();

                pGEOcoll[n - 1] = pChangePoints;
            }

            return pGEOcoll;

        }

        public void SectionAnalysis(string pLeftControlLine, string pRightControlLine, string pLeftRidgeLine, string pRightRidgeLine, string pDEMFile)
        {
            //获取DEM
            DEMRaster _raster = new DEMRaster(pDEMFile);
            //获取DEM的分辨率
            double pixel = _raster.GetPixel();

            #region 谷缘控制线
            Dictionary<int, Geometry> _Left3DControlLine = Create3DLines(pLeftControlLine, pixel, _raster);
            Dictionary<int, Geometry> _Right3DControlLine = Create3DLines(pRightControlLine, pixel, _raster);

            MorphingSolve.Create3dSurfaceByContour(_Left3DControlLine[0], _Right3DControlLine[0],"ContourLines", _raster);


            //(_Left3DControlLine.Values.ToList<Geometry>()).ExportGeometryToShapfile(_workspacePath, "leftV-7");
           // (_Right3DControlLine.Values.ToList<Geometry>()).ExportGeometryToShapfile(_workspacePath, "rightV-7");

            #region 生成堆栈剖面

            //StackProfile.CreateStackProfile(_Right3DControlLine[0], _workspacePath);

            #endregion

            #endregion

            #region 三维山脊线
            Dictionary<int, Geometry> _Left3DRidgeLine = Create3DLines(pLeftRidgeLine, pixel, _raster);
            Dictionary<int, Geometry> _Right3DRidgeLine = Create3DLines(pRightRidgeLine, pixel, _raster);

            //(_Left3DRidgeLine.Values.ToList<Geometry>()).ExportGeometryToShapfile(_workspacePath, "leftR-7");
            //(_Right3DRidgeLine.Values.ToList<Geometry>()).ExportGeometryToShapfile(_workspacePath, "rightR-7");

            #endregion

            #region 山脊线恢复 暂时不用
            //左侧二维山脊线
            Dictionary<int, Geometry> _2DLeftRidgeLineForReconvery = new Dictionary<int, Geometry>();// Dictionary<int, Geometry> _2DLeftRidgeLine = DataIO.GetAllControlLine(pLeftRidgeLine);;
            foreach (var id in _Left3DRidgeLine.Keys)
            {
                Geometry pLeft2DLine = new Geometry(wkbGeometryType.wkbLineString);
                for (int i = 0; i < _Left3DRidgeLine[id].GetPointCount(); i++)
                {
                    pLeft2DLine.AddPoint_2D(_Left3DRidgeLine[id].GetX(i), _Left3DRidgeLine[id].GetY(i));
                }

                _2DLeftRidgeLineForReconvery.Add(id, pLeft2DLine);
            }

            //右侧二维山脊线
            Dictionary<int, Geometry> _2DRightRidgeLineForRecovery = new Dictionary<int, Geometry>();// Dictionary<int, Geometry> _2DLeftRidgeLine = DataIO.GetAllControlLine(pLeftRidgeLine);;
            foreach (var id in _Right3DRidgeLine.Keys)
            {
                Geometry pRight2DLine = new Geometry(wkbGeometryType.wkbLineString);
                for (int i = 0; i < _Right3DRidgeLine[id].GetPointCount(); i++)
                {
                    pRight2DLine.AddPoint_2D(_Right3DRidgeLine[id].GetX(i), _Right3DRidgeLine[id].GetY(i));
                }

                _2DRightRidgeLineForRecovery.Add(id, pRight2DLine);
            }



            #endregion 

            #region 求Morphing需要的边缘控制线
            #region 左侧峡谷
            //二维控制线
            Dictionary<int, Geometry> _2DLeftControlLine = new Dictionary<int, Geometry>();// DataIO.GetAllControlLine(pLeftControlLine);
            foreach(var id in _Left3DControlLine.Keys)
            {
                Geometry pLeft2DLine = new Geometry(wkbGeometryType.wkbLineString);
                for (int i = 0; i < _Left3DControlLine[id].GetPointCount(); i++)
                {
                    pLeft2DLine.AddPoint_2D(_Left3DControlLine[id].GetX(i), _Left3DControlLine[id].GetY(i));
                }

                _2DLeftControlLine.Add(id, pLeft2DLine);
            }

            //二维山脊线
            Dictionary<int, Geometry> _2DLeftRidgeLine = new Dictionary<int, Geometry>();// Dictionary<int, Geometry> _2DLeftRidgeLine = DataIO.GetAllControlLine(pLeftRidgeLine);;
            foreach (var id in _Left3DRidgeLine.Keys)
            {
                Geometry pLeft2DLine = new Geometry(wkbGeometryType.wkbLineString);
                for (int i = 0; i < _Left3DRidgeLine[id].GetPointCount(); i++)
                {
                    pLeft2DLine.AddPoint_2D(_Left3DRidgeLine[id].GetX(i), _Left3DRidgeLine[id].GetY(i));
                }

                _2DLeftRidgeLine.Add(id, pLeft2DLine);
            }



            //山脊线和谷源控制线的交点
            List<Geometry> _2DLeftIntersectPoints = new List<Geometry>();

            foreach (var vl in _2DLeftControlLine.Keys)
            {
                foreach(var vt in _2DLeftRidgeLine.Keys)
                {
                    if(_2DLeftControlLine[vl].Intersect(_2DLeftRidgeLine[vt]))
                    {
                        Geometry intersectpt = _2DLeftControlLine[vl].Intersection(_2DLeftRidgeLine[vt]);
                        _2DLeftIntersectPoints.Add(intersectpt);
                    }           
                    //Geometry intersectpt=GeometrySolve.Get3DIntersectPoint(_2DLeftControlLine[vl], _2DLeftRidgeLine[vt], _raster);          
                }
            }


            #region 将线在交点处打断 分成多个部分

           List<Geometry> pLeftSegments = SplitLine(_2DLeftControlLine[0], _2DLeftRidgeLine);
           pLeftSegments.ExportGeometryToShapfile(_workspacePath, "segmentLineLeft");
            #endregion


            #endregion

            #region 右侧峡谷
            //二维控制线
            //Dictionary<int, Geometry> _2DRightControlLine = DataIO.GetAllControlLine(pRightControlLine);
            Dictionary<int, Geometry> _2DRightControlLine = new Dictionary<int, Geometry>();
            foreach (var id in _Right3DControlLine.Keys)
            {
                Geometry pRight2DLine = new Geometry(wkbGeometryType.wkbLineString);
                for (int i = 0; i < _Right3DControlLine[id].GetPointCount(); i++)
                {
                    pRight2DLine.AddPoint_2D(_Right3DControlLine[id].GetX(i), _Right3DControlLine[id].GetY(i));
                }

                _2DRightControlLine.Add(id, pRight2DLine);
            }
            //二维山脊线
            //Dictionary<int, Geometry> _2DRightRidgeLine = DataIO.GetAllControlLine(pRightRidgeLine);

            Dictionary<int, Geometry> _2DRightRidgeLine = new Dictionary<int, Geometry>();// Dictionary<int, Geometry> _2DLeftRidgeLine = DataIO.GetAllControlLine(pLeftRidgeLine);;
            foreach (var id in _Right3DRidgeLine.Keys)
            {
                Geometry pRight2DLine = new Geometry(wkbGeometryType.wkbLineString);
                for (int i = 0; i < _Right3DRidgeLine[id].GetPointCount(); i++)
                {
                    pRight2DLine.AddPoint_2D(_Right3DRidgeLine[id].GetX(i), _Right3DRidgeLine[id].GetY(i));
                }

                _2DRightRidgeLine.Add(id, pRight2DLine);
            }

            //山脊线和谷源控制线的交点
            List<Geometry> _2DRightIntersectPoints = new List<Geometry>();

            foreach (var vl in _2DRightControlLine.Keys)
            {
                foreach (var vt in _2DRightRidgeLine.Keys)
                {
                    if(_2DRightControlLine[vl].Intersect(_2DRightRidgeLine[vt]))
                    {
                        Geometry intersectpt = _2DRightControlLine[vl].Intersection(_2DRightRidgeLine[vt]);
                        _2DRightIntersectPoints.Add(intersectpt);
                    }       
                }
            }



            #region 将线在交点处打断 分成多个部分
            List<Geometry> pRightSegments = SplitLine(_2DRightControlLine[0], _2DRightRidgeLine);
            pRightSegments.ExportGeometryToShapfile(_workspacePath, "segmentLineRight");
            #endregion
            #endregion


            #region 测试线上的点的对应问题  成功

            //List<Geometry> _pRight = new List<Geometry>();
            //List<Geometry> _pLeft = new List<Geometry>();


            //if (_2DLeftControlLine[0].GetPointCount() > _2DRightControlLine[0].GetPointCount())
            //{
            //    MorphingSolve.GetCorrespondingGeo(_2DLeftControlLine[0], _2DRightControlLine[0], ref _pRight, ref _pLeft);

            //}
            //else
            //{
            //    MorphingSolve.GetCorrespondingGeo(_2DRightControlLine[0], _2DLeftControlLine[0], ref _pRight, ref _pLeft);
            //}

            #endregion




            #endregion

            #region 求变化控制线

            //1 先确定峡谷上方山脊线端点的距离和高度差

            double leftZ = _raster.GetElevation(_2DLeftIntersectPoints[0].GetX(0), _2DLeftIntersectPoints[0].GetY(0));
            double rightZ = _raster.GetElevation(_2DRightIntersectPoints[0].GetX(0), _2DRightIntersectPoints[0].GetY(0));
            //高度差
            double detaH = leftZ - rightZ;
            //距离
            double dist = Math.Sqrt(Math.Pow(_2DLeftIntersectPoints[0].GetX(0) - _2DRightIntersectPoints[0].GetX(0), 2) + Math.Pow(_2DLeftIntersectPoints[0].GetY(0) - _2DRightIntersectPoints[0].GetY(0), 2));

            //连接两侧峡谷的山脊线（现在还是直线段）
            Geometry pMiddleControlLine = new Geometry(wkbGeometryType.wkbLineString);
            pMiddleControlLine.AddPoint_2D(_2DLeftIntersectPoints[0].GetX(0), _2DLeftIntersectPoints[0].GetY(0));
            pMiddleControlLine.AddPoint_2D(_2DRightIntersectPoints[0].GetX(0), _2DRightIntersectPoints[0].GetY(0));
            //pMiddleControlLine.ExportSimpleGeometryToShapfile(_workspacePath, "Middleline");



            //2 通过推进半径为dist的圆的方式寻找最合适的山脊线

            //右边山脊线
            //2.1 正序
            double outMinH1 = 0.0;
            Geometry pCanyonChangeLinePos = CreateChangeLine(_2DRightRidgeLine[0], dist, detaH,_raster,ref outMinH1);

            //获取截取线的3D
            Geometry p3DCanyonChangeLinePos = GeometrySolve.Create3DLine(pCanyonChangeLinePos,_raster);
            p3DCanyonChangeLinePos.ExportSimpleGeometryToShapfile(_workspacePath, "3Dchangeline1");

            pCanyonChangeLinePos.ExportSimpleGeometryToShapfile(_workspacePath, "changeline1");
            double detaH1 = GeometrySolve.GetEvelationH(
                pCanyonChangeLinePos.GetX(0), 
                pCanyonChangeLinePos.GetY(0), 
                pCanyonChangeLinePos.GetX(pCanyonChangeLinePos.GetPointCount() - 1), 
                pCanyonChangeLinePos.GetY(pCanyonChangeLinePos.GetPointCount() - 1),_raster);

            //2.2 倒序
            double outMinH2 = 0.0;
            Geometry pCanyonChangeLineRev = CreateChangeLine(GeometrySolve.SwapLineOrder(_2DRightRidgeLine[0]), dist, detaH, _raster,ref outMinH2);
            pCanyonChangeLineRev.ExportSimpleGeometryToShapfile(_workspacePath, "changeline2");
            double detaH2 = GeometrySolve.GetEvelationH(
                pCanyonChangeLineRev.GetX(0),
                pCanyonChangeLineRev.GetY(0),
                pCanyonChangeLineRev.GetX(pCanyonChangeLinePos.GetPointCount() - 1),
                pCanyonChangeLineRev.GetY(pCanyonChangeLinePos.GetPointCount() - 1), _raster);

            //3 确定最后的变换线
            //Math.Abs(outMinH1) <= Math.Abs(outMinH2)
            if (true)
            {
                double s = 0;
                //处理pCanyonChangeLinePos
                //if ((detaH < 0&& detaH1<0)|| (detaH > 0 && detaH1 > 0))
                //{
                    #region 根据山脊线确定变换控制线上需要插入的点的位置

                    //获取线上每个点的比例
                    List<double> scales = GeometrySolve.GetInterpolatePointSclale(pCanyonChangeLinePos);

                    //记录每个点的实际高程和按照三角形比例计算的高程的差值
                    List<double> hDifference = GetElevationDifferetnce(scales, detaH1, pCanyonChangeLinePos,_raster);

                    //给pMiddleControlLine按照scales的比例插入点，并确定高程
                    Geometry p3DRidgeControlLine = GetRidgeControlLine(pMiddleControlLine,_raster, scales, hDifference);
                    p3DRidgeControlLine.ExportSimpleGeometryToShapfile(_workspacePath, "3DMiddleline");

                    //用Morphing插值技术确定曲面  三边法
                    for(int i=0;i<pLeftSegments.Count;i++)
                    {
                        
                        //将之前的谷缘控制线三维化
                        Geometry p3DLeftSegment = GeometrySolve.Create3DLine(pLeftSegments[i],_raster);
                        Geometry p3DRightSegment = GeometrySolve.Create3DLine(pRightSegments[i], _raster);

                        if (i == 0)
                        {

                            //最左边变化线
                            Geometry p3DLeftChangleLine = new Geometry(wkbGeometryType.wkbLineString);
                            p3DLeftChangleLine.AddPoint(p3DLeftSegment.GetX(0), p3DLeftSegment.GetY(0), p3DLeftSegment.GetZ(0));
                            p3DLeftChangleLine.AddPoint(p3DRightSegment.GetX(0), p3DRightSegment.GetY(0), p3DRightSegment.GetZ(0));
                            p3DLeftChangleLine = IntersectPointsToLineByScale(p3DLeftChangleLine, scales);

                            MorphingSolve.Create3dSurfaceByMorphing(p3DLeftSegment, p3DRightSegment, p3DLeftChangleLine, p3DRidgeControlLine, scales, "_3DinterPolateLines_U",_raster);
                      
                        }

                        if(i == (pLeftSegments.Count-1))
                        {
                            //最右边变化线
                            Geometry p3DRightChangleLine = new Geometry(wkbGeometryType.wkbLineString);
                            p3DRightChangleLine.AddPoint(p3DLeftSegment.GetX(p3DLeftSegment.GetPointCount()-1), p3DLeftSegment.GetY(p3DLeftSegment.GetPointCount() - 1), p3DLeftSegment.GetZ(p3DLeftSegment.GetPointCount() - 1));
                            p3DRightChangleLine.AddPoint(p3DRightSegment.GetX(p3DRightSegment.GetPointCount() - 1), p3DRightSegment.GetY(p3DRightSegment.GetPointCount() - 1), p3DRightSegment.GetZ(p3DRightSegment.GetPointCount() - 1));
                            p3DRightChangleLine = IntersectPointsToLineByScale(p3DRightChangleLine, scales);

                            MorphingSolve.Create3dSurfaceByMorphing(p3DLeftSegment, p3DRightSegment, p3DRidgeControlLine, p3DRightChangleLine, scales, "_3DinterPolateLines_D",_raster);


                        }
                    }




                    double gg = 0;

                    #endregion
                //}
            }
            else
            {
                //处理pCanyonChangeLineRev
                double s = 2;
            }

            #endregion

            double s22 = 0;
        }


        #region 山脊线相关函数


        /// <summary>
        /// 确定变化控制线的高程变化
        /// </summary>
        /// <param name="_pLine">原始直线</param>
        /// <param name="_raster">DEM高程</param>
        /// <param name="_scales">比例</param>
        /// <param name="_differenceH">高程差</param>
        /// <returns></returns>
        public Geometry GetRidgeControlLine(Geometry _pLine,DEMRaster _raster, List<double> _scales, List<double> _differenceH)
        {
            double startH = _raster.GetElevation(_pLine.GetX(0),_pLine.GetY(0));
            double endH = _raster.GetElevation(_pLine.GetX(1), _pLine.GetY(1));

            double detaH = endH-startH  ;

            //三维变化控制线
            Geometry _3DRidgeControlLine = new Geometry(wkbGeometryType.wkbLineString);
            _3DRidgeControlLine.AddPoint(_pLine.GetX(0), _pLine.GetY(0), startH);
            for (int i=0;i< _scales.Count;i++)
            {
                // Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
                double detax = (_pLine.GetX(1) - _pLine.GetX(0)) * _scales[i] + _pLine.GetX(0);

                double detay = (_pLine.GetY(1) - _pLine.GetY(0)) * _scales[i] + _pLine.GetY(0);

                double H = detaH * _scales[i] + startH+ _differenceH[i];

                _3DRidgeControlLine.AddPoint(detax, detay, H);
            }
            _3DRidgeControlLine.AddPoint(_pLine.GetX(1), _pLine.GetY(1), endH);

            return _3DRidgeControlLine;
        }


       
        /// <summary>
        /// 按照指定比例在直线上插入点
        /// </summary>
        /// <param name="_pLine">需要插入点的线</param>
        /// <param name="_scales">比例</param>
        /// <returns></returns>
        public Geometry IntersectPointsToLineByScale(Geometry _pLine, List<double> _scales)
        {
            double startH = _pLine.GetZ(0);
            double endH = _pLine.GetZ(1);

            double detaH = endH - startH;

            //三维变化控制线
            Geometry _3DRidgeControlLine = new Geometry(wkbGeometryType.wkbLineString);
            _3DRidgeControlLine.AddPoint(_pLine.GetX(0), _pLine.GetY(0), startH);
            for (int i = 0; i < _scales.Count; i++)
            {
                // Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
                double detax = (_pLine.GetX(1) - _pLine.GetX(0)) * _scales[i] + _pLine.GetX(0);

                double detay = (_pLine.GetY(1) - _pLine.GetY(0)) * _scales[i] + _pLine.GetY(0);

                double H = detaH * _scales[i] + startH;

                _3DRidgeControlLine.AddPoint(detax, detay, H);
            }
            _3DRidgeControlLine.AddPoint(_pLine.GetX(1), _pLine.GetY(1), endH);

            return _3DRidgeControlLine;
        }


        /// <summary>
        /// 根据点的数量插入道直线
        /// </summary>
        /// <param name="_pLine"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public Geometry IntersectPointsToLineByPoint(Geometry _pLine,int count)
        {

            List<double> _scales = new List<double>();
            for (int i=0;i<count;i++)
            {
                _scales.Add((i + 1.0) / Convert.ToDouble(count + 1));
            }

            double startH = _pLine.GetZ(0);
            double endH = _pLine.GetZ(1);

            double detaH = endH - startH;

            //三维变化控制线
            Geometry _3DRidgeControlLine = new Geometry(wkbGeometryType.wkbLineString);
            _3DRidgeControlLine.AddPoint(_pLine.GetX(0), _pLine.GetY(0), startH);
            for (int i = 0; i < _scales.Count; i++)
            {
                // Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
                double detax = (_pLine.GetX(1) - _pLine.GetX(0)) * _scales[i] + _pLine.GetX(0);

                double detay = (_pLine.GetY(1) - _pLine.GetY(0)) * _scales[i] + _pLine.GetY(0);

                double H = detaH * _scales[i] + startH;

                _3DRidgeControlLine.AddPoint(detax, detay, H);
            }
            _3DRidgeControlLine.AddPoint(_pLine.GetX(1), _pLine.GetY(1), endH);

            return _3DRidgeControlLine;
        }


        /// <summary>
        /// 获取高程差
        /// </summary>
        /// <param name="scales"></param>
        /// <param name="detaH"></param>
        /// <param name="_pLine"></param>
        /// <param name="_raster"></param>
        /// <returns></returns>
        public List<double> GetElevationDifferetnce(List<double> scales,double detaH,Geometry _pLine,DEMRaster _raster)
        {
            //高程差
            List<double> _hDifference = new List<double>();

            //获取三维线
            Geometry _3dLine = GeometrySolve.Create3DLine(_pLine, _raster);

            //获取每个点的高程差
            int count = _3dLine.GetPointCount();
            for(int i=0;i< scales.Count; i++)
            {
                double hOfLine = _3dLine.GetZ(0) + (-detaH)* scales[i];
                _hDifference.Add(_3dLine.GetZ(i+1)- hOfLine);
            }

            return _hDifference;
        }


        /// <summary>
        /// 获取山脊线上的控制变化线
        /// </summary>
        /// <param name="pRidgeLine"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private Geometry CreateChangeLine(Geometry pRidgeLine,double radius,double detaH, DEMRaster _raster, ref double _outMinH)
        {
            Geometry pChangeLine = new Geometry(wkbGeometryType.wkbLineString);

            double minDetaH = 10000000000000000000000.0;

            //不断缩小的山脊线
            Geometry pTransLine = new Geometry(wkbGeometryType.wkbLineString);

            //List<double> dest = new List<double>();

            for (int i=0;i< pRidgeLine.GetPointCount();i++)
            {
                if (i == 0)
                    pTransLine = pRidgeLine;

                //以山脊线上的点做缓冲区
                Geometry pt = new Geometry(wkbGeometryType.wkbPoint);        
                pt.AddPoint_2D(pTransLine.GetX(0), pTransLine.GetY(0));
                Geometry ptBuffer = pt.Buffer(radius, 0);
               // ptBuffer.ExportSimpleGeometryToShapfile(_workspacePath+"\\circle", "cicle"+i);

                //获取缓冲区的外边界
                //Geometry poutLine = ptBuffer.GetGeometryRef(0);
                //poutLine.ExportSimpleGeometryToShapfile(_workspacePath, "cicleline");
                //wkbGeometryType ptype1 = poutLine.GetGeometryType();

                //缓冲区和山脊线求交获得相交线
                
                Geometry pintesectLine = ptBuffer.Intersection(pTransLine);
                wkbGeometryType ptype = pintesectLine.GetGeometryType();

                //pintesectLine.ExportSimpleGeometryToShapfile(_workspacePath + "\\circle", "cicleline" + i);

                if (pintesectLine.Length() - pTransLine.Length() >=0)
                    break;
                
                //寻找最符合要求的山脊线
                double z1 = _raster.GetElevation(pintesectLine.GetX(0), pintesectLine.GetY(0));
                double z2 = _raster.GetElevation(pintesectLine.GetX(pintesectLine.GetPointCount()-1), pintesectLine.GetY(pintesectLine.GetPointCount() - 1));
                double detaZ = z1 - z2;
                double deH = Math.Abs(detaH)- Math.Abs(detaZ);

                //dest.Add(deH);

                if (deH < minDetaH)
                {
                    minDetaH = deH;
                    pChangeLine = pintesectLine;
                    _outMinH = deH;
                }

                pTransLine = new Geometry(wkbGeometryType.wkbLineString);
                for(int j=i+1; j < pRidgeLine.GetPointCount(); j++)
                {
                    pTransLine.AddPoint_2D(pRidgeLine.GetX(j), pRidgeLine.GetY(j));
                }               
            }
            return pChangeLine;
        }


        /// <summary>
        /// 将边缘控制线分段
        /// </summary>
        /// <param name="pControlLine"></param>
        /// <param name="pRidgeLine"></param>
        /// <returns></returns>
        private List<Geometry> SplitLine(Geometry pControlLine,Dictionary<int,Geometry> pRidgeLine)
        {

            Geometry pGotoLine = new Geometry(wkbGeometryType.wkbLineString);
            Geometry pLeftControlLineOnePart = new Geometry(wkbGeometryType.wkbLineString);
            Geometry pLeftControlLineAnotherPart = new Geometry(wkbGeometryType.wkbLineString);

            pGotoLine.AddPoint_2D(pControlLine.GetX(0), pControlLine.GetY(0));
            //pLeftControlLineOnePart.AddPoint_2D(_2DLeftControlLine[0].GetX(0), _2DLeftControlLine[0].GetY(0));

            //记录断掉的位置
            Dictionary<int, Geometry> intersectpoints = new Dictionary<int, Geometry>();
            //断点 
            int breakpt = 0;
            List<int> haveJudgeLine = new List<int>();
            for (int i = 1; i < pControlLine.GetPointCount(); i++)
            {
                pGotoLine.AddPoint_2D(pControlLine.GetX(i), pControlLine.GetY(i));

                foreach (var vt in pRidgeLine.Keys)
                {
                    if (haveJudgeLine.Contains(vt))
                        continue;

                    if (pGotoLine.Intersect(pRidgeLine[vt]))
                    {
                        haveJudgeLine.Add(vt);

                        Geometry intersectpt = pGotoLine.Intersection(pRidgeLine[vt]);

                        double s = intersectpt.GetX(0);
                        double ss = intersectpt.GetY(0);

                        intersectpoints.Add(i--, intersectpt);

                        breakpt++;
                    }
                }

            }

            //初始化多个分段
            List<Geometry> pSegments = new List<Geometry>(intersectpoints.Count + 1);
            for (int i = 0; i < (intersectpoints.Count + 1); i++)
            {
                Geometry psegment = new Geometry(wkbGeometryType.wkbLineString);
                pSegments.Add(psegment);
            }

            List<int> keyIndex = intersectpoints.Keys.ToList<int>();

            for (int i = 0; i < pSegments.Count; i++)
            {
                if (i == 0)
                {
                    for (int k = 0; k < keyIndex[0]; k++)
                    {
                        pSegments[i].AddPoint_2D(pControlLine.GetX(k), pControlLine.GetY(k));
                    }

                    double s = intersectpoints[keyIndex[0]].GetX(0);
                    double ss = intersectpoints[keyIndex[0]].GetY(0);
                    pSegments[i].AddPoint_2D(intersectpoints[keyIndex[0]].GetX(0), intersectpoints[keyIndex[0]].GetY(0));
                }

                else if (i == (pSegments.Count - 1))
                {
                    pSegments[i].AddPoint_2D(intersectpoints[keyIndex[i - 1]].GetX(0), intersectpoints[keyIndex[i - 1]].GetY(0));

                    for (int k = (keyIndex[keyIndex.Count - 1] + 1); k < pControlLine.GetPointCount(); k++)
                    {
                        pSegments[i].AddPoint_2D(pControlLine.GetX(k), pControlLine.GetY(k));
                    }
                }

                else
                {
                    pSegments[0].AddPoint_2D(intersectpoints[keyIndex[i - 1]].GetX(0), intersectpoints[keyIndex[i - 1]].GetY(0));

                    for (int k = (keyIndex[i - 1] + 1); k < keyIndex[i]; k++)
                    {
                        pSegments[i].AddPoint_2D(pControlLine.GetX(k), pControlLine.GetY(k));
                    }

                    pSegments[i].AddPoint_2D(intersectpoints[keyIndex[i]].GetX(0), intersectpoints[keyIndex[i]].GetY(0));
                }
            }

           

            return pSegments;
        }

        #endregion



        #region 常规函数
        /// <summary>
        /// 给某个线上按照距离pixel添加点并转为三维线
        /// </summary>
        /// <param name="pLineFile"></param>
        /// <param name="pixel"></param>
        /// <param name="_raster"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Dictionary<int, Geometry> Create3DLines(string pLineFile, double pixel, DEMRaster _raster)
        {
            //提取线
            Dictionary<int, Geometry> _Line = DataIO.GetAllControlLine(pLineFile);
            //_Line[0].ExportSimpleGeometryToShapfile(_workspacePath, "leftline");

            //在线上插入点
            Dictionary<int, Geometry> _3DControlLines = new Dictionary<int, Geometry>();
            foreach (var id in _Line.Keys)
            {
                //给线上插入点
                Geometry intePoLine = GeometrySolve.InsertPointsToLine(_Line[id], pixel);

                //三维化线
                _3DControlLines.Add(id, GeometrySolve.Create3DLine(intePoLine, _raster));
            }
            // _copyControlLine[0].ExportSimpleGeometryToShapfile(_workspacePath, "leftline2d");
            //(_3DControlLine.Values.ToList<Geometry>()).ExportGeometryToShapfile(_workspacePath, name);

            return _3DControlLines;
        }

        #endregion

    }
}
