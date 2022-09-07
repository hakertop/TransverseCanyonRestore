using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanyonRecoveryEntry
{
    public static class ClassExtensionMethod
    {

        /// <summary>
        /// 将单个几何输出到要素类
        /// </summary>
        /// <param name="_triMesh"></param>
        /// <param name="_workSpacePath"></param>
        /// <param name="_fileName"></param>
        public static void ExportSimpleGeometryToShapfile(this Geometry geometryCollection, string _workSpacePath, string _fileName)
        {
            //注册Ogr库
            string pszDriverName = "ESRI Shapefile";
            //调用对Shape文件读写的Driver接口
            OSGeo.OGR.Driver poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);
            if (poDriver == null)
                throw new Exception("Driver Error");

            // 为了支持中文路径，请添加下面这句代码
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            // 为了使属性表字段支持中文，请添加下面这句
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");


            //1、创建数据源
            OSGeo.OGR.DataSource poDS;
            poDS = poDriver.CreateDataSource(_workSpacePath + "\\" + _fileName + ".shp", null);//如果原始文件夹内有该要素数据，会覆盖。
            if (poDS == null)
                throw new Exception("DataSource Creation Error");

            OSGeo.OSR.SpatialReference oSRS = new OSGeo.OSR.SpatialReference("");
            oSRS.SetWellKnownGeogCS("WGS84");
            oSRS.SetUTM(48,1);

            //3、创建层Layer
            Layer poLayer = poDS.CreateLayer(_fileName, oSRS, geometryCollection.GetGeometryType(), null);
            if (poLayer == null)
                throw new Exception("Layer Creation Failed");



            FieldDefn oFieldID = new FieldDefn("FieldID", FieldType.OFTInteger);
            poLayer.CreateField(oFieldID, 1);

            //创建一个Feature,一个Polygon
            Feature poFeature = new Feature(poLayer.GetLayerDefn());


            poFeature.SetField(0, 0);

            poFeature.SetGeometry(geometryCollection);

            poLayer.CreateFeature(poFeature);


            poDS.Dispose();
            poLayer.Dispose();
            poFeature.Dispose();
        }


        /// <summary>
        /// 将多个Geometry输出到要素类
        /// </summary>
        /// <param name="_triMesh"></param>
        /// <param name="_workSpacePath"></param>
        /// <param name="_fileName"></param>
        public static void ExportGeometryToShapfile(this List<Geometry> geometryCollection, string _workSpacePath, string _fileName)
        {
            if (geometryCollection.Count <= 0)
                return;

            //注册Ogr库
            string pszDriverName = "ESRI Shapefile";
            //调用对Shape文件读写的Driver接口
            OSGeo.OGR.Driver poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);
            if (poDriver == null)
                throw new Exception("Driver Error");

            //// 为了支持中文路径，请添加下面这句代码
            //OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            //// 为了使属性表字段支持中文，请添加下面这句
            //OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");


            //1、创建数据源
            OSGeo.OGR.DataSource poDS;
            poDS = poDriver.CreateDataSource(_workSpacePath + "\\" + _fileName + ".shp", null);//如果原始文件夹内有该要素数据，会覆盖。
            if (poDS == null)
                throw new Exception("DataSource Creation Error");

            //3、创建层Layer

            Layer poLayer = poDS.CreateLayer(_fileName, null, geometryCollection[0].GetGeometryType(), null);
            if (poLayer == null)
                throw new Exception("Layer Creation Failed");

            FieldDefn oFieldID = new FieldDefn("FieldID", FieldType.OFTInteger);
            poLayer.CreateField(oFieldID, 1);

            //创建一个Feature,
            Feature poFeature = new Feature(poLayer.GetLayerDefn());

            for (int i = 0; i < geometryCollection.Count; i++)
            {

                poFeature.SetField(0, i);

                poFeature.SetGeometry(geometryCollection[i]);

                poLayer.CreateFeature(poFeature);

            }
            poDS.Dispose();
            poLayer.Dispose();
            poFeature.Dispose();
        }


        /// <summary>
        /// 将多个Geometry输出到要素类
        /// </summary>
        /// <param name="_triMesh"></param>
        /// <param name="_workSpacePath"></param>
        /// <param name="_fileName"></param>
        public static void Export3DPoints(this List<Geometry> geometryCollection, string _workSpacePath, string _fileName)
        {
            if (geometryCollection.Count <= 0)
                return;

            //注册Ogr库
            string pszDriverName = "ESRI Shapefile";
            //调用对Shape文件读写的Driver接口
            OSGeo.OGR.Driver poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);
            if (poDriver == null)
                throw new Exception("Driver Error");

            //// 为了支持中文路径，请添加下面这句代码
            //OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            //// 为了使属性表字段支持中文，请添加下面这句
            //OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");


            //1、创建数据源
            OSGeo.OGR.DataSource poDS;
            poDS = poDriver.CreateDataSource(_workSpacePath + "\\" + _fileName + ".shp", null);//如果原始文件夹内有该要素数据，会覆盖。
            if (poDS == null)
                throw new Exception("DataSource Creation Error");

            //3、创建层Layer

            Layer poLayer = poDS.CreateLayer(_fileName, null, wkbGeometryType.wkbPoint, null);
            if (poLayer == null)
                throw new Exception("Layer Creation Failed");

            FieldDefn oFieldID = new FieldDefn("FieldID", FieldType.OFTInteger);
            poLayer.CreateField(oFieldID, 1);

            FieldDefn oFieldID_1 = new FieldDefn("z", FieldType.OFTReal);
            poLayer.CreateField(oFieldID_1, 1);

            //创建一个Feature,
            Feature poFeature = new Feature(poLayer.GetLayerDefn());

            for (int i = 0; i < geometryCollection.Count; i++)
            {
                Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
                pt.AddPoint_2D(geometryCollection[i].GetX(0), geometryCollection[i].GetY(0));


                poFeature.SetField(0, i);
                poFeature.SetField(1, geometryCollection[i].GetZ(0));

                poFeature.SetGeometry(pt);

                poLayer.CreateFeature(poFeature);

            }
            poDS.Dispose();
            poLayer.Dispose();
            poFeature.Dispose();
        }

        /// <summary>
        /// 创建矢量数据
        /// </summary>
        /// <param name="_workSpacePath"></param>
        /// <param name="_fileName"></param>
        public static void ExportTriMeshToShapfile(string _workSpacePath, string _fileName)
        {

            //注册Ogr库
            string pszDriverName = "ESRI Shapefile";
            //调用对Shape文件读写的Driver接口
            OSGeo.OGR.Driver poDriver = OSGeo.OGR.Ogr.GetDriverByName(pszDriverName);
            if (poDriver == null)
                throw new Exception("Driver Error");

            // 为了支持中文路径，请添加下面这句代码
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            // 为了使属性表字段支持中文，请添加下面这句
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");


            //1、创建数据源
            OSGeo.OGR.DataSource poDS;
            poDS = poDriver.CreateDataSource(_workSpacePath + "\\" + _fileName + ".shp", null);//如果原始文件夹内有该要素数据，会覆盖。
            if (poDS == null)
                throw new Exception("DataSource Creation Error");

            //2、创建层Layer
            Layer poLayer = poDS.CreateLayer(_fileName, null, wkbGeometryType.wkbPoint, null);
            if (poLayer == null)
                throw new Exception("Layer Creation Failed");

            //3创建字段
            FieldDefn oFieldID = new FieldDefn("FieldID", FieldType.OFTInteger);
            poLayer.CreateField(oFieldID, 1);

            //4创建一个Feature,一个point
            Feature poFeature = new Feature(poLayer.GetLayerDefn());
            for (int i = 0; i <= 5; i++)
            {
                Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
                pt.AddPoint_2D(i * 3.14, i * 3.14);

                //输出到表中
                poFeature.SetField(0, i);
                poFeature.SetGeometry(pt);

                poLayer.CreateFeature(poFeature);
            }

            //释放数据集，如果程序运行过程中需要对参见的要素类继续读写，必须释放
            poDS.Dispose();
            poLayer.Dispose();
            poFeature.Dispose();
        }

    }
}
