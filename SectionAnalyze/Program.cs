

using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GdalLib;

using CanyonRecoveryEntry;
using System.IO;
using GeoCommon;

namespace CanyonRecovery
{
     class Program
    {
        static void Main(string[] args)
        {
            Gdal.AllRegister();
            Ogr.RegisterAll();
            // 为了支持中文路径，请添加下面这句代码
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            // 为了使属性表字段支持中文，请添加下面这句
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "UTF-8");

            string pLeftControlLine = @"E:\MyEssay\CanyonRecovery\Documentation\Result\3\data\LeftV.shp";
            string pRightControlLine = @"E:\MyEssay\CanyonRecovery\Documentation\Result\3\data\RightV.shp";

            string pLeftRidgeLine = @"E:\MyEssay\CanyonRecovery\Documentation\Result\3\data\LeftR.shp";
            string pRightRidgeLine = @"E:\MyEssay\CanyonRecovery\Documentation\Result\3\data\RightR.shp";

            string pDEM = @"E:\MyEssay\CanyonRecovery\Documentation\Result\4\newdem.tif";

            string workSpace = @"E:\MyEssay\CanyonRecovery\Documentation\Result\4";



            #region 生成三维网格曲面
            //string p3DTransLine1 = @"E:\MyEssay\CanyonRecovery\Documentation\morphing\data\_3DinterPolateLines_D.shp";
            //string p3DTransLine2 = @"E:\MyEssay\CanyonRecovery\Documentation\morphing\data\_3DinterPolateLines_U.shp";

            ////第一条
            //List<Geometry> ptras1 = LayerHelp.GetGeometryList(p3DTransLine1).Values.ToList<Geometry>();
            //List<Geometry> tripolys1 = new List<Geometry>();
            //for (int o = 0; o < ptras1.Count - 1; o++)
            //{
            //    int t = o+1;
            //    MorphingSolve.ConstructTriangle(ptras1[o], ptras1[t], ref tripolys1);
            //}
            //tripolys1.ExportGeometryToShapfile(@"E:\MyEssay\CanyonRecovery\Documentation\morphing\data", "TR");

            ////第二条
            //List<Geometry> ptras2 = LayerHelp.GetGeometryList(p3DTransLine2).Values.ToList<Geometry>();
            //List<Geometry> tripolys2 = new List<Geometry>();
            //for (int o = 0; o < ptras2.Count - 1; o++)
            //{
            //    int t = o + 1;
            //    MorphingSolve.ConstructTriangle(ptras2[o], ptras2[t], ref tripolys2);
            //}
            //tripolys2.ExportGeometryToShapfile(@"E:\MyEssay\CanyonRecovery\Documentation\morphing\data", "TU");
            #endregion

            #region 输出所有三维点 为了对边服务
            //List<Geometry> allpoints = new List<Geometry>();
            //string p3DTransLine1 = workSpace + @"\ContourLines.shp";
            //GetAllPoints(p3DTransLine1, ref allpoints);

            //DEMRaster _raster = new DEMRaster(pDEM);

            //for (int i = 0; i < allpoints.Count; i++)
            //{
            //    if (allpoints[i].GetZ(0) < _raster.GetElevation(allpoints[i].GetX(0), allpoints[i].GetY(0)))
            //    {
            //        Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
            //        pt.AddPoint(allpoints[i].GetX(0), allpoints[i].GetY(0), _raster.GetElevation(allpoints[i].GetX(0), allpoints[i].GetY(0)));
            //        allpoints[i] = pt;
            //    }
            //}
            //allpoints.Export3DPoints(workSpace, "ALLpointsbyContour");

            #endregion


            #region 输出所有三维点 


            //List<Geometry> allpoints = new List<Geometry>();

            //string p3DTransLine1 = workSpace + @"\_3DinterPolateLines_D.shp";
            //string p3DTransLine2 = workSpace + @"\_3DinterPolateLines_U.shp";

            //GetAllPoints(p3DTransLine1, ref allpoints);
            //GetAllPoints(p3DTransLine2, ref allpoints);
            //GetAllPoints(pLeftControlLine, ref allpoints);
            //GetAllPoints(pRightControlLine, ref allpoints);

            //DEMRaster _raster = new DEMRaster(pDEM);

            //for (int i = 0; i < allpoints.Count; i++)
            //{
            //    if (allpoints[i].GetZ(0) < _raster.GetElevation(allpoints[i].GetX(0), allpoints[i].GetY(0)))
            //    {
            //        Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
            //        pt.AddPoint(allpoints[i].GetX(0), allpoints[i].GetY(0), _raster.GetElevation(allpoints[i].GetX(0), allpoints[i].GetY(0)));
            //        allpoints[i] = pt;
            //    }
            //}
            //allpoints.Export3DPoints(workSpace, "ALLpoints");

            #endregion

            #region 测试山脊线提取成功  但是不完善

            //string pRidgeLines = @"E:\MyEssay\CanyonRecovery\StudyData\wuxia\Ridgeline\15mRidgeline1.shp";

            //RidgePointExtract rpe = new RidgePointExtract(pRidgeLines, pDEM);
            //rpe.CreateCharacteristicPoints();

            #endregion

            #region 横断峡谷三维模型构建

            string pUpPointsFile = @"E:\MyEssay\CanyonRecovery\Documentation\Result\4\Uppoints1.shp";
            string pDownPointsFile = @"E:\MyEssay\CanyonRecovery\Documentation\Result\4\OutBasePoints.shp";
            string pBoundaryPointsFile_1 = @"E:\MyEssay\CanyonRecovery\Documentation\Result\4\BoundarPoints-1.shp";
            string pBoundaryPointsFile_2 = @"E:\MyEssay\CanyonRecovery\Documentation\Result\4\BoundarPoints-2.shp";
            string pBoundaryPointsFile_3 = @"E:\MyEssay\CanyonRecovery\Documentation\Result\4\BoundarPoints-3.shp";
            string pBoundaryPointsFile_4 = @"E:\MyEssay\CanyonRecovery\Documentation\Result\4\BoundarPoints-4.shp";

            Dictionary<Geometry, double> _upPoints = LayerHelp.GetAll3DPoints(pUpPointsFile);
            Dictionary<Geometry, double> _downPoints = LayerHelp.GetAll3DPoints(pDownPointsFile);
            Dictionary<Geometry, double> _boundaryPoints_1 = LayerHelp.GetAll3DPoints(pBoundaryPointsFile_1);
            Dictionary<Geometry, double> _boundaryPoints_2 = LayerHelp.GetAll3DPoints(pBoundaryPointsFile_2);
            Dictionary<Geometry, double> _boundaryPoints_3 = LayerHelp.GetAll3DPoints(pBoundaryPointsFile_3);
            Dictionary<Geometry, double> _boundaryPoints_4 = LayerHelp.GetAll3DPoints(pBoundaryPointsFile_4);

           

            for(int i= _boundaryPoints_4.Keys.Count-1;i>=0;i--)
            {
                _boundaryPoints_1.Add(_boundaryPoints_4.Keys.ToList()[i], _boundaryPoints_4[_boundaryPoints_4.Keys.ToList()[i]]);
            } 
            
            for(int i= _boundaryPoints_2.Keys.Count-1;i>=0;i--)
            {
                _boundaryPoints_1.Add(_boundaryPoints_2.Keys.ToList()[i], _boundaryPoints_2[_boundaryPoints_2.Keys.ToList()[i]]);
            }

           
             

            foreach (var vt in _boundaryPoints_3.Keys)
                _boundaryPoints_1.Add(vt, _boundaryPoints_3[vt]);

            _boundaryPoints_1.Keys.ToList().Export3DPoints(workSpace, "ALLpointsbyContour");

            CreateMesh(_upPoints, _boundaryPoints_1);
            #endregion


            CanyonMain pCM = new CanyonMain(workSpace);

            #region 溯源侵蚀
            //List<Geometry>[] plll = pCM.Headwarderosion(pLeftControlLine, pRightControlLine, allpoints, pDEM);
            //for (int i = 0; i < plll.Length; i++)
            //{
            //    plll[i].Export3DPoints(workSpace, "GEOpoints" + i);
            //}
            #endregion

            pCM.SectionAnalysis(pLeftControlLine, pRightControlLine, pLeftRidgeLine, pRightRidgeLine,pDEM);

            Console.WriteLine("运行成功");
        }


        /// <summary>
        /// 提取线上的点
        /// </summary>
        /// <param name="_file"></param>
        /// <param name="_pallpoints"></param>
        public static void GetAllPoints(string _file, ref List<Geometry> _pallpoints)
        {         
            List<Geometry> ptras2 = LayerHelp.GetGeometryList(_file).Values.ToList<Geometry>();
            for (int i = 0; i < ptras2.Count; i++)
            {
                for (int k = 0; k < ptras2[i].GetPointCount(); k++)
                {
                    Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
                    pt.AddPoint(ptras2[i].GetX(k), ptras2[i].GetY(k), ptras2[i].GetZ(k));

                    _pallpoints.Add(pt);
                }
            }
        }

        /// <summary>
        /// 创建三角网
        /// </summary>
        /// <param name="points"></param>
        /// <param name="boundarypoints"></param>
        public static void CreateMesh(Dictionary<Geometry, double> points, Dictionary<Geometry, double> boundarypoints)
        {
            //2 约束选项（约束类）
            var options = new TriangleNet.Meshing.ConstraintOptions();
            options.SegmentSplitting = 2;
            options.ConformingDelaunay = true;
            options.Convex = false;

            //3 质量选项（质量类）
            var quality = new TriangleNet.Meshing.QualityOptions();


            Dictionary<string, double> pointZvalue = new Dictionary<string, double>();

            TriangleNet.Geometry.Contour pNewCon = null;// new TriangleNet.Geometry.Contour(pv);


            //4 确定离散点
            TriangleNet.Geometry.IPolygon input = GetPolygon(points, boundarypoints, ref pointZvalue,ref pNewCon);

            //5 添加边界约束
            input.Add(pNewCon, false);

            //6 构网
            TriangleNet.Mesh mesh = null;
            if (input != null)
            {
                mesh = (TriangleNet.Mesh)TriangleNet.Geometry.ExtensionMethods.Triangulate(input, options);
            }

            TriMesh trimesh = new TriMesh();
            foreach (var tri in mesh.Triangles)
            {
                TriangleNet.Geometry.Vertex v1 = tri.GetVertex(0);
                TriangleNet.Geometry.Vertex v2 = tri.GetVertex(1);
                TriangleNet.Geometry.Vertex v3 = tri.GetVertex(2);

                trimesh.AddTriangle(v1.X, v1.Y, pointZvalue[v1.X+"-"+ v1.Y], v2.X, v2.Y, pointZvalue[v2.X + "-" + v2.Y], v3.X, v3.Y, pointZvalue[v3.X + "-" + v3.Y]);
            }


            //try
            //{
            using (StreamWriter sw = new StreamWriter(@"E:\MyEssay\CanyonRecovery\Documentation\Result\4\1.obj"))
                {
                    //sw.WriteLine("o Head");
                    //sw.WriteLine("g default");

                    string tl = string.Empty;

                 
                    for (int i = 0; i < trimesh.vertexList.Count; i++)
                    {
                  

                        tl = "v " + Convert.ToString(trimesh.vertexList[i].x) + " " + Convert.ToString(trimesh.vertexList[i].y) + " " + Convert.ToString(trimesh.vertexList[i].z);
                        sw.WriteLine(tl);
                    }


                    sw.WriteLine("s off");
                    //sw.WriteLine("g tringles");
                    sw.WriteLine("g " + "1");

                    for (int i = 0; i < trimesh.triangleList.Count; i++)
                    {
                       
                        tl = "f " + Convert.ToString(trimesh.triangleList[i].v0 + 1) + " " + Convert.ToString(trimesh.triangleList[i].v1 + 1) + " "
                            + Convert.ToString(trimesh.triangleList[i].v2+ 1);
                        sw.WriteLine(tl);
                    }

                    sw.Close();
                }
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("表面模型信息转成OBJ格式失败" + ex.ToString());
            //}


          

        }


        /// <summary>
        /// 获取弯曲线上所有的点 用于构网
        /// </summary>
        /// <param name="drillList"></param>
        /// <returns></returns>
        private static TriangleNet.Geometry.IPolygon GetPolygon(Dictionary<Geometry, double> points, 
            Dictionary<Geometry, double> boundarypoints,
            ref Dictionary<string, double> zValues,
            ref TriangleNet.Geometry.Contour pcon)
        {
            TriangleNet.Geometry.IPolygon data = new TriangleNet.Geometry.Polygon();

            foreach (var vt in points.Keys)
            {
                TriangleNet.Geometry.Vertex triVertex = new TriangleNet.Geometry.Vertex(vt.GetX(0), vt.GetY(0));
                if (zValues.ContainsKey(vt.GetX(0)+"-"+vt.GetY(0)))
                    continue;

                zValues.Add(vt.GetX(0) + "-" + vt.GetY(0), points[vt]);
                // triVertex.ID = vt.id;
                data.Add(triVertex);
            }

            List<TriangleNet.Geometry.Vertex> pv = new List<TriangleNet.Geometry.Vertex>();
            foreach (var vt in boundarypoints.Keys)
            {
                TriangleNet.Geometry.Vertex triVertex = new TriangleNet.Geometry.Vertex(vt.GetX(0), vt.GetY(0));

                if (zValues.ContainsKey(vt.GetX(0) + "-" + vt.GetY(0)))
                    continue;

                zValues.Add(vt.GetX(0) + "-" + vt.GetY(0), boundarypoints[vt]);
                // triVertex.ID = vt.id;
                data.Add(triVertex);
                pv.Add(triVertex);
            }

            pcon = new TriangleNet.Geometry.Contour(pv);

            return data;
        }

    }
}
