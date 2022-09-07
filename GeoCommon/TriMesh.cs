using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GeoCommon
{
    public class TriMesh
    {

        /// <summary>
        /// ID
        /// </summary>
        public int id;

        /// <summary>
        /// 三角网名称
        /// </summary>
        public string name;
        /// <summary>
        /// 三角网顶点列表
        /// </summary>
        public IList<Vertex> vertexList;

        /// <summary>
        /// 三角网三角形列表
        /// </summary>
        public IList<Triangle> triangleList;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TriMesh()
        {
            name = "未命名";

            vertexList = new List<Vertex>();

            triangleList = new List<Triangle>();
        }

        /// <summary>
        /// 添加顶点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public int AddVertex(double x, double y, double z)
        {

            //注意：每次添加顶点时要保证是新的节点
            Vertex geoVertex = new Vertex();
            geoVertex.id = vertexList.Count;
            //geoVertex.x = Math.Round(x, 2);//保留两位小数
            //geoVertex.y = Math.Round(y, 2);
            //geoVertex.z = Math.Round(z, 2);

            geoVertex.x = x;//保留两位小数
            geoVertex.y = y;
            geoVertex.z = z;

            foreach (var vertex in vertexList)
            {
                if (Math.Abs(vertex.x - geoVertex.x) < 0.0001 &&
                    Math.Abs(vertex.y - geoVertex.y) < 0.0001 &&
                    Math.Abs(vertex.z - geoVertex.z) < 0.0001)
                {
                    return vertex.id;
                }
            }

            vertexList.Add(geoVertex);

            return geoVertex.id;
        }

        /// <summary>
        /// 向三角网模型中添加一个三角形--形式一
        /// </summary>
        /// <param name="id">三角形ID</param>
        /// <param name="v0">顶点ID</param>
        /// <param name="v1">顶点ID</param>
        /// <param name="v2">顶点ID</param>
        public void AddTriangle(int id, int v0, int v1, int v2)
        {
            Triangle geoTri = new Triangle();

            geoTri.id = id;
            geoTri.v0 = v0;
            geoTri.v1 = v1;
            geoTri.v2 = v2;

            triangleList.Add(geoTri);
        }

        /// <summary>
        /// 向三角网模型中添加一个三角形--形式二
        /// </summary>
        /// <param name="x0">顶点1X坐标</param>
        /// <param name="y0">顶点1Y坐标</param>
        /// <param name="z0">顶点1Z坐标</param>
        /// <param name="x1">顶点2X坐标</param>
        /// <param name="y1">顶点2Y坐标</param>
        /// <param name="z1">顶点2Z坐标</param>
        /// <param name="x2">顶点3X坐标</param>
        /// <param name="y2">顶点3Y坐标</param>
        /// <param name="z2">顶点3Z坐标</param>
        public void AddTriangle(double x0, double y0, double z0,
            double x1, double y1, double z1, double x2, double y2, double z2)
        {
            int gv0 = AddVertex(x0, y0, z0);
            int gv1 = AddVertex(x1, y1, z1);
            int gv2 = AddVertex(x2, y2, z2);

            Triangle geoTri = new Triangle();
            geoTri.id = triangleList.Count;
            geoTri.v0 = gv0;
            geoTri.v1 = gv1;
            geoTri.v2 = gv2;
            triangleList.Add(geoTri);
        }

        /// <summary>
        /// 获取包含点的所有三角形
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>符合条件的三角网集合</returns>
        public List<Triangle> getTriangleByCoord(double x, double y)
        {
            List<Triangle> triBedrock = new List<Triangle>();

            for (int i = 0; i < triangleList.Count; i++)
            {
                Triangle tri = triangleList[i];

                if ((Math.Round(vertexList[tri.v0].x, 2) == Math.Round(x, 2) && Math.Round(vertexList[tri.v0].y, 2) == Math.Round(y, 2))
                        || (Math.Round(vertexList[tri.v1].x, 2) == Math.Round(x, 2) && Math.Round(vertexList[tri.v1].y, 2) == Math.Round(y, 2))
                        || (Math.Round(vertexList[tri.v2].x, 2) == Math.Round(x, 2) && Math.Round(vertexList[tri.v2].y, 2) == Math.Round(y, 2)))
                    triBedrock.Add(tri);
            }

            return triBedrock;
        }

        /// <summary>
        /// 输出三角网信息
        /// </summary>
        /// <param name="_workspacePath">工作路径</param>
        /// <param name="fileName">文件名称</param>
        public void ExportTriMesh(string _workspacePath, string fileName)
        {
            StreamWriter streamWriter = new StreamWriter(_workspacePath + "\\" + fileName + ".txt");
            streamWriter.WriteLine(vertexList.Count.ToString() + " " + triangleList.Count.ToString());
            for (int i = 0; i < vertexList.Count; i++)
            {
                Vertex v = vertexList[i];
                streamWriter.WriteLine(v.x.ToString() + " " + v.y.ToString() + " " + v.z.ToString());
            }
            for (int i = 0; i < triangleList.Count; i++)
            {
                Triangle tri = triangleList[i];
                streamWriter.WriteLine("3 " + tri.v0.ToString() + " " + tri.v1.ToString() + " " + tri.v2.ToString());
            }

            streamWriter.Close();
        }
    }
}
