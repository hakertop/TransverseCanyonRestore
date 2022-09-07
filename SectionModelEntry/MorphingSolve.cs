using GeoCommon;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdalLib;

namespace CanyonRecoveryEntry
{
    public  class MorphingSolve
    {
        //变化线
        //public static  List<Geometry> _3DinterPolateLines = new List<Geometry>();

        public static string workspacefile = @"E:\MyEssay\CanyonRecovery\Documentation\Result\3\output";

        public static void Create3dSurfaceByMorphing(Geometry SRC, Geometry DEST,Geometry leftControl,Geometry rightControl,List<double> scales,string _name,DEMRaster _raster)
        {
            //将起始边和终止边的上的点对应起来
            List<Geometry> _pRight = new List<Geometry>();
            List<Geometry> _pLeft = new List<Geometry>();        
            if (SRC.GetPointCount() > DEST.GetPointCount())
            {
                MorphingSolve.GetCorrespondingGeo(SRC, DEST, ref _pRight, ref _pLeft);
            }
            else
            {
                MorphingSolve.GetCorrespondingGeo(DEST, SRC, ref _pLeft, ref _pRight);        
            }


            //建立高度对应关系
            Dictionary<int, double> zHLeft = new Dictionary<int, double>();
            Dictionary<int, double> zHRight = new Dictionary<int, double>();
            
            double deLeftz = leftControl.GetZ(leftControl.GetPointCount() - 1) - leftControl.GetZ(0);
            double deRightz = rightControl.GetZ(rightControl.GetPointCount() - 1) - rightControl.GetZ(0);

            for (int i=1;i< leftControl.GetPointCount()-1;i++)
            {
                
                double ptLeftz = leftControl.GetZ(0) + deLeftz * scales[i-1];
                double detaLeftZ = leftControl.GetZ(i) - ptLeftz;

                
                double ptRightz = rightControl.GetZ(0) + deRightz * scales[i-1];
                double detaRightZ = rightControl.GetZ(i) - ptRightz;

                zHLeft.Add(i-1, detaLeftZ);
                zHRight.Add(i-1, detaRightZ);
            }

            //过渡曲线
            List<Geometry> _3DinterPolateLines = new List<Geometry>();

            for (int k = 0; k < scales.Count; k++)
            {
                Geometry interPolateLine = new Geometry(wkbGeometryType.wkbLineString);

                Geometry _3DinterPolateLine = new Geometry(wkbGeometryType.wkbLineString);

                interPolateLine.AddPoint_2D(leftControl.GetX(k + 1), leftControl.GetY(k + 1));


                for (int i = 0; i < _pRight.Count; i++)
                {
                    double srcX = _pRight[i].GetX(0);
                    double srcY = _pRight[i].GetY(0);
                    double srcZ = _pRight[i].GetZ(0);

                    double destX = _pLeft[i].GetX(0);
                    double destY = _pLeft[i].GetY(0);
                    double destZ = _pLeft[i].GetZ(0);

                    double srcXdestX = destX - srcX;
                    double srcYdestY = destY - srcY;
                    double srcZdestZ = destZ - srcZ;


                    double inteX = srcX + scales[k] * srcXdestX;
                    double inteY = srcY + scales[k] * srcYdestY;
                    double inteZ = srcZ + scales[k] * srcZdestZ;

                    interPolateLine.AddPoint_2D(inteX, inteY);

                }
                interPolateLine.AddPoint_2D(rightControl.GetX(k + 1), rightControl.GetY(k + 1));

                //获取比例
                List<Double> newScales = GeometrySolve.GetInterpolatePointSclale(interPolateLine);

                _3DinterPolateLine.AddPoint(leftControl.GetX(k + 1), leftControl.GetY(k + 1), leftControl.GetZ(k + 1));
                for (int j = 0; j < _pRight.Count; j++)
                {
                    
                    double srcZ = _pRight[j].GetZ(0);
                    double destZ = _pLeft[j].GetZ(0);
                    double srcZdestZ = destZ - srcZ;

                    //插值Z值
                    double inteZ = srcZ + scales[k] * srcZdestZ;

                    //实际Z值destZ
                    double trueZ = (1 - newScales[j]) * zHLeft[k] + newScales[j] * zHRight[k] + inteZ;

                    //插值点处的高程
                    double ele = _raster.GetElevation(interPolateLine.GetX(j + 1), interPolateLine.GetY(j + 1));
                    if(trueZ > ele)
                    {
                        
                    }
                    else
                    {
                        trueZ = ele;
                    }
                        
                    _3DinterPolateLine.AddPoint(interPolateLine.GetX(j + 1), interPolateLine.GetY(j + 1), trueZ);          
                }
                

                _3DinterPolateLine.AddPoint(rightControl.GetX(k + 1), rightControl.GetY(k + 1), rightControl.GetZ(k + 1));


                _3DinterPolateLines.Add(_3DinterPolateLine);
                //_3DinterPolateLine.ExportSimpleGeometryToShapfile(@"E:\MyEssay\CanyonRecovery\StudyData\wuxia\UnitTest", "_3DinterPolateLine"+k);

            }

            _3DinterPolateLines.ExportGeometryToShapfile(workspacefile, _name);
        }

        /// <summary>
        /// 根据轮廓线方法构建曲面
        /// </summary>
        /// <param name="SRC"></param>
        /// <param name="DEST"></param>
        /// <param name="_name"></param>
        /// <param name="_raster"></param>
        public static void Create3dSurfaceByContour(Geometry SRC, Geometry DEST, string _name, DEMRaster _raster)
        {
            //将起始边和终止边的上的点对应起来
            List<Geometry> _pRight = new List<Geometry>();
            List<Geometry> _pLeft = new List<Geometry>();
            if (SRC.GetPointCount() > DEST.GetPointCount())
            {
                MorphingSolve.GetCorrespondingGeo_1(SRC, DEST, ref _pRight, ref _pLeft);
            }
            else
            {
                MorphingSolve.GetCorrespondingGeo_1(DEST, SRC, ref _pLeft, ref _pRight);
            }

            //过渡曲线
            List<Geometry> _3DinterPolateLines = new List<Geometry>();

            for (int k = 1; k < 191; k++)
            {
                Geometry interPolateLine = new Geometry(wkbGeometryType.wkbLineString);

                for (int i = 0; i < _pRight.Count; i++)
                {
                    double srcX = _pRight[i].GetX(0);
                    double srcY = _pRight[i].GetY(0);
                    double srcZ = _pRight[i].GetZ(0);

                    double destX = _pLeft[i].GetX(0);
                    double destY = _pLeft[i].GetY(0);
                    double destZ = _pLeft[i].GetZ(0);

                    double srcXdestX = destX - srcX;
                    double srcYdestY = destY - srcY;
                    double srcZdestZ = destZ - srcZ;


                    double inteX = srcX + (k / 191.0) * srcXdestX;
                    double inteY = srcY + (k / 191.0) * srcYdestY;
                    double inteZ = srcZ + (k / 191.0) * srcZdestZ;

                    double ele = _raster.GetElevation(inteX, inteY);


                    if (inteZ < ele)
                        inteZ = ele;

                    interPolateLine.AddPoint(inteX, inteY, inteZ);
                   
                }

                _3DinterPolateLines.Add(interPolateLine);
                
                //_3DinterPolateLine.ExportSimpleGeometryToShapfile(@"E:\MyEssay\CanyonRecovery\StudyData\wuxia\UnitTest", "_3DinterPolateLine"+k);
            }
            _3DinterPolateLines.Add(SRC);
            _3DinterPolateLines.Add(DEST);
            _3DinterPolateLines.ExportGeometryToShapfile(workspacefile, _name);
        }



        /// <summary>
        /// 确定线上的对应点
        /// </summary>
        /// <param name="SRC"></param>
        /// <param name="DEST"></param>
        /// <param name="ptpts"></param>
        public static void GetCorrespondingGeo(Geometry SRC, Geometry DEST,ref List<Geometry> pOneside, ref List<Geometry> pAnotherSide)
        {
            List<Geometry> pSRCs = new List<Geometry>(SRC.GetPointCount()-2);
            for (int i = 1; i < SRC.GetPointCount() - 1; i++)
            {
                Geometry pt1 = new Geometry(wkbGeometryType.wkbPoint);
                pt1.AddPoint(SRC.GetX(i), SRC.GetY(i), SRC.GetZ(i));

                pSRCs.Add(pt1);
            }

            List<Geometry> pDESTs = new List<Geometry>(DEST.GetPointCount() - 2);
            for (int i = 1; i < DEST.GetPointCount() - 1; i++)
            {
                Geometry pt2 = new Geometry(wkbGeometryType.wkbPoint);
                pt2.AddPoint(DEST.GetX(i), DEST.GetY(i), DEST.GetZ(i));

                pDESTs.Add(pt2);
            }

            //获取对应关系
            Dictionary<int, string> corpts = ConstructTri(pSRCs, pDESTs);


            foreach (var index in corpts.Keys)
            {
                string pts = corpts[index];

                string[] gt2 = pts.Split('-');

                int ind1 = Convert.ToInt16(gt2[0]);
                int ind2 = Convert.ToInt16(gt2[1])-(SRC.GetPointCount() - 2);


                pOneside.Add(pSRCs[ind1]);
                pAnotherSide.Add(pDESTs[ind2]);
                
            }

            pOneside.TrimExcess();
            pAnotherSide.TrimExcess();

            List<Geometry> PLINES = new List<Geometry>();

            for (int i=0;i<pOneside.Count;i++)
            {
                Geometry pline = new Geometry(wkbGeometryType.wkbLineString);
                pline.AddPoint_2D(pOneside[i].GetX(0), pOneside[i].GetY(0));
                pline.AddPoint_2D(pAnotherSide[i].GetX(0), pAnotherSide[i].GetY(0));
                PLINES.Add(pline);
            }

            PLINES.ExportGeometryToShapfile(workspacefile, "triPolateLine");

        }

        /// <summary>
        /// 确定线上的对应点
        /// </summary>
        /// <param name="SRC"></param>
        /// <param name="DEST"></param>
        /// <param name="ptpts"></param>
        public static void GetCorrespondingGeo_1(Geometry SRC, Geometry DEST, ref List<Geometry> pOneside, ref List<Geometry> pAnotherSide)
        {
            List<Geometry> pSRCs = new List<Geometry>();
            for (int i = 0; i < SRC.GetPointCount(); i++)
            {
                Geometry pt1 = new Geometry(wkbGeometryType.wkbPoint);
                pt1.AddPoint(SRC.GetX(i), SRC.GetY(i), SRC.GetZ(i));

                pSRCs.Add(pt1);
            }

            List<Geometry> pDESTs = new List<Geometry>();
            for (int i = 0; i < DEST.GetPointCount(); i++)
            {
                Geometry pt2 = new Geometry(wkbGeometryType.wkbPoint);
                pt2.AddPoint(DEST.GetX(i), DEST.GetY(i), DEST.GetZ(i));

                pDESTs.Add(pt2);
            }

            //获取对应关系
            Dictionary<int, string> corpts = ConstructTri(pSRCs, pDESTs);


            foreach (var index in corpts.Keys)
            {
                string pts = corpts[index];

                string[] gt2 = pts.Split('-');

                int ind1 = Convert.ToInt16(gt2[0]);
                int ind2 = Convert.ToInt16(gt2[1]) - SRC.GetPointCount();


                pOneside.Add(pSRCs[ind1]);
                pAnotherSide.Add(pDESTs[ind2]);

            }

            pOneside.TrimExcess();
            pAnotherSide.TrimExcess();

            List<Geometry> PLINES = new List<Geometry>();

            for (int i = 0; i < pOneside.Count; i++)
            {
                Geometry pline = new Geometry(wkbGeometryType.wkbLineString);
                pline.AddPoint_2D(pOneside[i].GetX(0), pOneside[i].GetY(0));
                pline.AddPoint_2D(pAnotherSide[i].GetX(0), pAnotherSide[i].GetY(0));
                PLINES.Add(pline);
            }

            PLINES.ExportGeometryToShapfile(workspacefile, "triPolateLine");

        }

        /// <summary>
        /// 利用最小距离法确定两条线上点的对应关系
        /// </summary>
        /// <param name="pSrc"></param>
        /// <param name="pDest"></param>
        /// <returns></returns>
        public static Dictionary<int, string> ConstructTri(List<Geometry> pSrc, List<Geometry> pDest)
        {

            //便于记录 通过Vertex2D来代替Geometry
            List<Vertex2D> v2DSrc = new List<Vertex2D>(pSrc.Count);
           for(int k=0;k< pSrc.Count;k++)
            {
                Vertex2D pt2d = new Vertex2D(pSrc[k].GetX(0), pSrc[k].GetY(0));
                pt2d.id = k;
                v2DSrc.Add(pt2d);
            }

            List<Vertex2D> v2DDest = new List<Vertex2D>(pDest.Count);
            for (int k = 0; k < pDest.Count; k++)
            {
                Vertex2D pt2d = new Vertex2D(pDest[k].GetX(0), pDest[k].GetY(0));
                pt2d.id = (k+ pSrc.Count);
                v2DDest.Add(pt2d);
            }


            int j = 0;
            int i = 0;
            int flag = 0;
            int index = 0;

            bool order = true;
            

            Dictionary<int, string> pCorres = new Dictionary<int, string>();
            pCorres.Add(index, v2DSrc[0].id+"-"+ v2DDest[0].id);

            //循环 这里默认 pDest.Coun<pSrc.Count
            while ((i+1)< pDest.Count)
            {
                index++;

                double dst1 = pSrc[j].Distance(pDest[i + 1]);
                double dst2 = pSrc[j+1].Distance(pDest[i]);

                if (dst1 <= dst2)
                {
                    pCorres.Add(index, v2DSrc[j].id+"-"+ v2DDest[i + 1].id);
                    i++;
                }
                else
                {
                    pCorres.Add(index, v2DSrc[j+1].id+"-"+ v2DDest[i].id);
                    j++;
                }

                if ((j + 1) >= pSrc.Count)
                {
                    flag = i;
                    order = true;

                    break;
                }
                else
                {
                    order = false;
                    flag = j;
                }
            }

            //pSrc上多出来的点直接和pDest的末尾点相对应
            if (!order)
            {
                for (int k = j + 1; k < pSrc.Count; k++)
                {
                    index++;
                    pCorres.Add(index, v2DSrc[k].id + "-" + v2DDest[pDest.Count - 1].id);
                }
            }
            //pDest上多出来的点直接和pSrc的末尾点相对应
            else
            {
                for (int k = i + 1; k < pDest.Count; k++)
                {
                    index++;
                    pCorres.Add(index, v2DSrc[pSrc.Count - 1].id + "-" + v2DDest[k].id);
                }
            }

            return pCorres;
        }

        /// <summary>
        /// 构建三角网
        /// </summary>
        /// <param name="pSrc"></param>
        /// <param name="pDest"></param>
        /// <param name="triPolygongs"></param>
        public static void ConstructTriangle(Geometry SRC, Geometry DEST, ref List<Geometry> triPolygongs)
        {

            List<Geometry> pSrc = new List<Geometry>(SRC.GetPointCount());
            for (int p = 0; p < SRC.GetPointCount(); p++)
            {
                Geometry pt1 = new Geometry(wkbGeometryType.wkbPoint);
                pt1.AddPoint(SRC.GetX(p), SRC.GetY(p), SRC.GetZ(p));

                pSrc.Add(pt1);
            }

            List<Geometry> pDest = new List<Geometry>(DEST.GetPointCount());
            for (int p = 0; p < DEST.GetPointCount() ; p++)
            {
                Geometry pt2 = new Geometry(wkbGeometryType.wkbPoint);
                pt2.AddPoint(DEST.GetX(p), DEST.GetY(p), DEST.GetZ(p));

                pDest.Add(pt2);
            }


            int i = 0;
            int j = 0;
            int flag = 0;

            bool order = true;

            while ((i + 1) < pDest.Count)
            {

                double dst1 = pSrc[j].Distance(pDest[i + 1]);
                double dst2 = pSrc[j + 1].Distance(pDest[i]);

                if (dst1 <= dst2)
                {
                    Geometry ring = new Geometry(wkbGeometryType.wkbLinearRing);
                    ring.AddPoint(pDest[i].GetX(0), pDest[i].GetY(0), pDest[i].GetZ(0));
                    ring.AddPoint(pSrc[j].GetX(0), pSrc[j].GetY(0), pSrc[j].GetZ(0));
                    ring.AddPoint(pDest[i + 1].GetX(0), pDest[i + 1].GetY(0), pDest[i + 1].GetZ(0));
                    ring.AddPoint(pDest[i].GetX(0), pDest[i].GetY(0), pDest[i].GetZ(0));

                    Geometry pPolygon = new Geometry(wkbGeometryType.wkbPolygonZM);
                    pPolygon.AddGeometry(ring);

                    triPolygongs.Add(pPolygon);

                    i++;
                }
                else
                {
                    Geometry ring = new Geometry(wkbGeometryType.wkbLinearRing);
                    ring.AddPoint(pSrc[j].GetX(0), pSrc[j].GetY(0), pSrc[j].GetZ(0));                   
                    ring.AddPoint(pDest[i].GetX(0), pDest[i].GetY(0), pDest[i].GetZ(0));
                    ring.AddPoint(pSrc[j + 1].GetX(0), pSrc[j + 1].GetY(0), pSrc[j + 1].GetZ(0));
                    ring.AddPoint(pSrc[j].GetX(0), pSrc[j].GetY(0), pSrc[j].GetZ(0));
                    Geometry pPolygon = new Geometry(wkbGeometryType.wkbPolygonZM);
                    pPolygon.AddGeometry(ring);

                    triPolygongs.Add(pPolygon);
                    j++;
                }

                if ((j + 1) >= pSrc.Count)
                {
                    flag = i;
                    order = true;

                    break;
                }
                else
                {
                    order = false;
                    flag = j;
                }
            }

            //pSrc上多出来的点直接和pDest的末尾点相对应
            if (!order)
            {
                for (int k = j + 1; k < pSrc.Count-1; k++)
                {
                    Geometry ring = new Geometry(wkbGeometryType.wkbLinearRing);
                    ring.AddPoint(pSrc[k].GetX(0), pSrc[k].GetY(0), pSrc[k].GetZ(0));
                    ring.AddPoint(pDest[pDest.Count - 1].GetX(0), pDest[pDest.Count - 1].GetY(0), pDest[pDest.Count - 1].GetZ(0));
                    ring.AddPoint(pSrc[k + 1].GetX(0), pSrc[k + 1].GetY(0), pSrc[k + 1].GetZ(0));
                    ring.AddPoint(pSrc[k].GetX(0), pSrc[k].GetY(0), pSrc[k].GetZ(0));
                    Geometry pPolygon = new Geometry(wkbGeometryType.wkbPolygonZM);
                    pPolygon.AddGeometry(ring);

                    triPolygongs.Add(pPolygon);
                }
            }
            //pDest上多出来的点直接和pSrc的末尾点相对应
            else
            {
                for (int k = i + 1; k < pDest.Count-1; k++)
                {

                    Geometry ring = new Geometry(wkbGeometryType.wkbLinearRing);
                    ring.AddPoint(pSrc[pSrc.Count - 1].GetX(0), pSrc[pSrc.Count - 1].GetY(0), pSrc[pSrc.Count - 1].GetZ(0));
                    ring.AddPoint(pDest[k].GetX(0), pDest[k].GetY(0), pDest[k].GetZ(0));
                    ring.AddPoint(pDest[k + 1].GetX(0), pDest[k + 1].GetY(0), pDest[k + 1].GetZ(0));
                    ring.AddPoint(pSrc[pSrc.Count - 1].GetX(0), pSrc[pSrc.Count - 1].GetY(0), pSrc[pSrc.Count - 1].GetZ(0));
                    Geometry pPolygon = new Geometry(wkbGeometryType.wkbPolygonZM);
                    pPolygon.AddGeometry(ring);

                    triPolygongs.Add(pPolygon);
                }
            }

        }
    }
}
