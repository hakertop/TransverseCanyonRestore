using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeoCommon
{
    /// <summary>
    /// 面的分段边
    /// </summary>
    public class Eage2D
    {

        /// <summary>
        /// ID
        /// </summary>
        public int id;

        /// <summary>
        /// 边界名称
        /// </summary>
        public string name;


        /// <summary>
        /// 顶点集合
        /// </summary>
        public List<Vertex2D> vertexList;

        /// <summary>
        /// shp格式的边
        /// </summary>
        public Geometry GeoPolyline { get; set; }


        /// <summary>
        /// 构造函数
        /// </summary>
        public Eage2D()
        {
            
            vertexList = new List<Vertex2D>();
            Geometry GeoPolyline = new Geometry(wkbGeometryType.wkbLineString);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pGeoPolyline"></param>
        /// <param name="pPolylineName"></param>
        public Eage2D(Geometry pGeoPolyline)
        {
            
            vertexList = new List<Vertex2D>();
            GeoPolyline = pGeoPolyline;
        }

        /// <summary>
        /// 添加顶点
        /// </summary>
        /// <param name="modelVertex"></param>
        /// <returns></returns>
        public int AddVertex(Vertex2D polygonVertex)
        {
            //注意：每次添加顶点时要保证是新的节点
            Vertex2D geoVertex = new Vertex2D();
            geoVertex.name = polygonVertex.name;
            geoVertex.id = vertexList.Count;

            geoVertex.x = polygonVertex.x;//保留两位小数
            geoVertex.y = polygonVertex.y;
          

            foreach (var vertex in vertexList)
            {
                if (Math.Abs(vertex.x - geoVertex.x) < 0.01 &&
                    Math.Abs(vertex.y - geoVertex.y) < 0.01)
                {
                    return vertex.id;
                }
            }

            vertexList.Add(geoVertex);

            return geoVertex.id;
        }


        /// <summary>
        /// 添加顶点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public int AddVertex(double x, double y)
        {

            //注意：每次添加顶点时要保证是新的节点
            Vertex2D geoVertex = new Vertex2D();
            geoVertex.id = vertexList.Count;
            //geoVertex.x = Math.Round(x, 2);//保留两位小数
            //geoVertex.y = Math.Round(y, 2);

            geoVertex.x = x;
            geoVertex.y = y;

            foreach (var vertex in vertexList)
            {
                if (Math.Abs(vertex.x - geoVertex.x) < 0.01 &&
                    Math.Abs(vertex.y - geoVertex.y) < 0.01)
                {
                    return vertex.id;
                }
            }

            vertexList.Add(geoVertex);

            return geoVertex.id;
        }


        /// <summary>
        /// 获取三维平面的中心点坐标
        /// </summary>
        /// <param name="pEageA"></param>
        /// <param name="pEageB"></param>
        public Vertex2D Get3DCentralPoint()
        {
            double distX = 0;
            double distY = 0;

            foreach (var item in this.vertexList)
            {
                distX = distX + item.x;
                distY = distY + item.y;
            }

            int count = this.vertexList.Count;
            Vertex2D vt = new Vertex2D(distX / count, distY / count);

            return vt;
        }


        /// <summary>
        /// 获取二维平面的中心点坐标
        /// </summary>
        /// <param name="pEageA"></param>
        /// <param name="pEageB"></param>
        public Vertex2D Get2DCentralPoint()
        {
            double distX = 0;
            double distY = 0;

            foreach (var item in this.vertexList)
            {
                distX = distX + item.x;
                distY = distY + item.y;
            }

            int count = this.vertexList.Count;
            Vertex2D vt = new Vertex2D(distX / count, distY / count);

            return vt;
        }

        /// <summary>
        /// 获取边的长度
        /// </summary>
        /// <returns></returns>
        public double GetLength()
        {
            return GeoPolyline.Length();
        }

    }
}
