using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeologicalEntity;
using OSGeo.OGR;
using OSGeo.OSR;
using GdalLib;
using GeoCommon;

namespace CanyonRecoveryEntry
{
    public class DataIO
    {
        #region 获取所有地层信息
        /// <summary>
        /// 获取所有地层
        /// </summary>
        /// <param name="pLayFile"></param>
        /// <returns></returns>
        public static Dictionary<int, Stratum> GetAllStrata(string pLayFile)
        {
            Dictionary<int, Stratum> pStrata = new Dictionary<int, Stratum>();

            Layer pContourLayer = LayerHelp.GetLayerByLayerName(System.IO.Path.GetDirectoryName(pLayFile), System.IO.Path.GetFileNameWithoutExtension(pLayFile));
            int PFeatureCount = (int)pContourLayer.GetFeatureCount(0);
            for (int i = 0; i < PFeatureCount; i++)
            {
                string name = pContourLayer.GetFeature(i).GetFieldAsString("S_Name");
                string id = pContourLayer.GetFeature(i).GetFieldAsString("Id");
                Geometry pg = pContourLayer.GetFeature(i).GetGeometryRef();
                //初始化一个地层
                Stratum pStratum = new Stratum() ;
                pStratum.SID = Convert.ToInt16(id);
                pStratum.SName = name;

                //获取地层的几何要素  
                pStratum.SPolygon = GetStratumPolygon(pg, pStratum.SName);

                pStrata.Add(pStratum.SID, pStratum);
            }

            return pStrata;
        }

        /// <summary>
        /// 将几何要素转换为地层几何面
        /// </summary>
        /// <param name="pGeo"></param>
        /// <param name="pName"></param>
        /// <returns></returns>
        private static Polygon2D GetStratumPolygon(Geometry pGeo, string pName)
        {
            //新建一个Polygon
            Polygon2D pStratumPoly = new Polygon2D(pGeo, pName);
            wkbGeometryType PT = pGeo.GetGeometryType();               

            if (PT == wkbGeometryType.wkbPolygon || PT == wkbGeometryType.wkbPolygon25D)
            {
                //边的数量
                int eageCount = pGeo.GetGeometryCount();

                for (int i = 0; i < eageCount; i++)
                {
                    Eage2D pEage = new Eage2D();
                    Geometry pGeoEage = pGeo.GetGeometryRef(i);
                    pEage.name = pName + i;

                    int count = pGeoEage.GetPointCount();

                    for (int j = 0; j < pGeoEage.GetPointCount(); j++)
                    {

                        Vertex2D vt = new Vertex2D();
                        vt.name = pEage.name + pEage.vertexList.Count;
                        vt.x = (float)pGeoEage.GetX(j);
                        vt.y = (float)pGeoEage.GetY(j);
                       //vt.z = pGeoEage.GetZ(j);

                        pEage.AddVertex(vt);

                        pStratumPoly.AddVertex(vt);
                    }
                    
                    pStratumPoly.AddEage(pEage);
                }
            }

            return pStratumPoly;
        }

        #endregion

        #region 获取所有谷缘线信息
        /// <summary>
        /// 获取谷缘控制线
        /// </summary>
        /// <param name="pLayFile"></param>
        /// <returns></returns>
        public static Dictionary<int, Geometry> GetAllControlLine(string pLayFile)
        {
            Dictionary<int, Geometry> pControlLines = new Dictionary<int, Geometry>();

            Layer pContourLayer = LayerHelp.GetLayerByLayerName(System.IO.Path.GetDirectoryName(pLayFile), System.IO.Path.GetFileNameWithoutExtension(pLayFile));
            int PFeatureCount = (int)pContourLayer.GetFeatureCount(0);
            for (int i = 0; i < PFeatureCount; i++)
            {
                int lineid = pContourLayer.GetFeature(i).GetFieldAsInteger("Id");             
                //string lineName = pContourLayer.GetFeature(i).GetFieldAsString("name");
                Geometry pl = pContourLayer.GetFeature(i).GetGeometryRef();
                

                wkbGeometryType PT = pl.GetGeometryType();
                if (PT == wkbGeometryType.wkbLineString || PT == wkbGeometryType.wkbLineString25D)
                    pControlLines.Add(lineid, pl);

            }
            
            return pControlLines;
        }


     

        /// <summary>
        /// 输出褶皱线的几何格式
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="sName"></param>
        /// <returns></returns>
        private static Eage2D GetFaultPolyline(Geometry pl, string sName)
        {
            wkbGeometryType PT = pl.GetGeometryType();
            

            if (PT == wkbGeometryType.wkbLineString || PT == wkbGeometryType.wkbLineString25D)
            {
                Eage2D pEage2D = new Eage2D(pl);
                pEage2D.name = sName;
                int count = pl.GetPointCount();
                for (int j = 0; j < count; j++)
                {
                    Vertex2D vt = new Vertex2D();
                    vt.name = pEage2D.name + pEage2D.vertexList.Count;
                    vt.x = (float)pl.GetX(j);
                    vt.y = (float)pl.GetY(j);

                    pEage2D.AddVertex(vt);

                }
                return pEage2D;
            }
            else
            {
                Console.WriteLine("输入褶皱线格式有误！！");
                return null;
            }
        }
        #endregion
    }
}
