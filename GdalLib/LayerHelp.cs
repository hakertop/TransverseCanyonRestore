using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GdalLib
{
   public static class LayerHelp
    {
        /// <summary>
        /// 根据要素路径与要素名获取要素
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static Layer GetLayerByLayerName(string filePath, string layerName)
        {
            DataSource ds = Ogr.Open(filePath, 0);
            if (ds == null)
            {
                throw new Exception("不能打开" + filePath);
            }

            OSGeo.OGR.Driver drv = ds.GetDriver();
            if (drv == null)
            {
                throw new Exception("不能获取驱动，请检查！");
            }

            Layer drillLayer = ds.GetLayerByName(layerName);
            if (drillLayer == null)
                throw new Exception("获取要素失败");

            return drillLayer;
        }


        /// <summary>
        /// 根据要素路径要素
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static Layer GetLayerByLayerName(string pFilePath)
        {
            string filePath = System.IO.Path.GetDirectoryName(pFilePath);
            string layerName = System.IO.Path.GetFileNameWithoutExtension(pFilePath);
            DataSource ds = Ogr.Open(filePath, 0);
            if (ds == null)
            {
                throw new Exception("不能打开" + filePath);
            }

            OSGeo.OGR.Driver drv = ds.GetDriver();
            if (drv == null)
            {
                throw new Exception("不能获取驱动，请检查！");
            }

            Layer drillLayer = ds.GetLayerByName(layerName);
            if (drillLayer == null)
                throw new Exception("获取要素失败");

            return drillLayer;
        }


         /// <summary>
        /// 获取要素类里的所有几何
        /// </summary>
        /// <param name="pLayFile"></param>
        /// <returns></returns>
        public static Dictionary<int, Geometry> GetGeometryList(string pLayFile)
        {
            Dictionary<int, Geometry> pGeometry = new Dictionary<int, Geometry>();

            Layer pContourLayer = LayerHelp.GetLayerByLayerName(System.IO.Path.GetDirectoryName(pLayFile), System.IO.Path.GetFileNameWithoutExtension(pLayFile));
            int PFeatureCount = (int)pContourLayer.GetFeatureCount(0);
            for (int i = 0; i < PFeatureCount; i++)
            {
                //int id = pContourLayer.GetFeature(i).GetFieldAsInteger("Id");
                Geometry pg = pContourLayer.GetFeature(i).GetGeometryRef();
                pGeometry.Add(i, pg);
            }
            return pGeometry;
        }


        /// <summary>
        /// 获取点数据
        /// </summary>
        /// <param name="pLayFile"></param>
        /// <returns></returns>
        public static Dictionary<Geometry, double> GetAll3DPoints(string pLayFile)
        {
            Dictionary<Geometry, double> pGeometry = new Dictionary<Geometry, double>();

            Layer pContourLayer = LayerHelp.GetLayerByLayerName(System.IO.Path.GetDirectoryName(pLayFile), System.IO.Path.GetFileNameWithoutExtension(pLayFile));
            int PFeatureCount = (int)pContourLayer.GetFeatureCount(0);
            for (int i = 0; i < PFeatureCount; i++)
            {
                double z = pContourLayer.GetFeature(i).GetFieldAsDouble("GRID_CODE");
                Geometry pg = pContourLayer.GetFeature(i).GetGeometryRef();
                pGeometry.Add(pg, z);

            }
            return pGeometry;

        }

     
    }
}
