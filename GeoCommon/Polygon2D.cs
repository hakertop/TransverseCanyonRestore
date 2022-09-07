using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;


namespace GeoCommon
{
    /// <summary>
    /// 地层面类
    /// </summary>
    public class Polygon2D
    {
        /// <summary>
        /// ID
        /// </summary>
        public int id;

        /// <summary>
        /// 面名称
        /// </summary>
        public string name;
        /// <summary>
        /// 顶点列表
        /// </summary>
        public List<Vertex2D> vertexList;

        /// <summary>
        /// 边界列表
        /// </summary>
        public List<Eage2D> eageList;

        /// <summary>
        /// 最大厚度
        /// </summary>
        public double SMaxT { get; set; }

        /// <summary>
        /// 最小厚度
        /// </summary>
        public double SMinT { get; set; }


        /// <summary>
        /// shp格式的多边形
        /// </summary>
        public Geometry GeoPolygon { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Polygon2D()
        {
            name = "未命名";

            vertexList = new List<Vertex2D>();

            eageList = new List<Eage2D>();

            GeoPolygon = new Geometry(wkbGeometryType.wkbPolygon);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Polygon2D(Geometry pGeoPolygon,String pPolygonName)
        {
            name = pPolygonName;

            vertexList = new List<Vertex2D>();

            eageList = new List<Eage2D>();

            GeoPolygon = pGeoPolygon;
        }



        /// <summary>
        /// 添加顶点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public int AddVertex(Vertex2D modelVertex)
        {
            //注意：每次添加顶点时要保证是新的节点
            Vertex2D geoVertex = new Vertex2D();
            geoVertex.id = vertexList.Count;
            //geoVertex.name = modelVertex.name;
            //geoVertex.x = Math.Round(modelVertex.x, 2);//保留两位小数
            //geoVertex.y = Math.Round(modelVertex.y, 2);

            geoVertex.x = modelVertex.x;//保留两位小数
            geoVertex.y = modelVertex.y;

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
            geoVertex.x = x ;//保留两位小数
            geoVertex.y = y ;

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
        /// 添加边界
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void AddEage(Eage2D modelEage)
        {
            eageList.Add(modelEage);
        }

        /// <summary>
        /// 添加边界
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void AddEage(List<Vertex2D> listVertex)
        {
            Eage2D eage = new Eage2D();
            eage.id = eageList.Count;
     
            eageList.Add(eage);
        }

        /// <summary>
        /// 获取多边形的面积
        /// </summary>
        /// <returns></returns>
       public float GetArea()
        {
           return (float)GeoPolygon.GetArea();
        }



    }
}
