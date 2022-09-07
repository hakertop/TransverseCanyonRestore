using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GdalLib
{
    public class DEMRaster
    {
        /// <summary>
        /// 栅格数据的保存路径
        /// </summary>
        private string _rasterFilePath;

        /// <summary>
        /// 栅格数据集
        /// </summary>
        public Dataset _ds;

        public DEMRaster()
        {
        }

        public DEMRaster(string filePath)
        {
            _rasterFilePath = filePath;
            //新建栅格数据集
            _ds = Gdal.Open(_rasterFilePath, Access.GA_ReadOnly);
        }


        public DEMRaster(string directoryPath, string fileName)
        {
            _rasterFilePath = directoryPath + "\\" + fileName;
            //新建栅格数据集
            _ds = Gdal.Open(_rasterFilePath, Access.GA_ReadOnly);
        }


        /// <summary>
        /// 获取分辨率
        /// </summary>
        /// <returns></returns>
        public double GetPixel()
        {
            double[] adfGeoTransform = new double[6];
            _ds.GetGeoTransform(adfGeoTransform);

            return adfGeoTransform[1];
        }


        /// <summary>
        /// 获取DEM上指定点的高程值
        /// </summary>
        /// <param name="dProjX"></param>
        /// <param name="dProjY"></param>
        /// <returns></returns>
        public double GetElevation(double dProjX, double dProjY)
        {
            try
            {
                Band Band = _ds.GetRasterBand(1);

                //获取图像的尺寸               
                int width = Band.XSize;
                int height = Band.YSize;

                //获取坐标变换系数
                double[] adfGeoTransform = new double[6];
                _ds.GetGeoTransform(adfGeoTransform);
                //获取行列号
                double dTemp = adfGeoTransform[1] * adfGeoTransform[5] - adfGeoTransform[2] * adfGeoTransform[4];
                double dCol = 0.0, dRow = 0.0;
                dCol = (adfGeoTransform[5] * (dProjX - adfGeoTransform[0]) -
                    adfGeoTransform[2] * (dProjY - adfGeoTransform[3])) / dTemp + 0.5;
                dRow = (adfGeoTransform[1] * (dProjY - adfGeoTransform[3]) -
                    adfGeoTransform[4] * (dProjX - adfGeoTransform[0])) / dTemp + 0.5;
                int dc = Convert.ToInt32(dCol);
                int dr = Convert.ToInt32(dRow);


                //获取DEM数值到一维数组
                double[] data = new double[1 * 1];
                CPLErr err = Band.ReadRaster(dc, dr, 1, 1, data, 1, 1, 0, 0);
                Band.Dispose();
                double elvate = data[0];
                return elvate;
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// 栅格插值
        /// </summary>
        /// <param name="path"></param>
        /// <param name="layerName"></param>
        public static void TestGdalGrid(string _zfd, string path, string layerName, string tifName, double[] extend, int innerstep)
        {
            string[] opt = new string[] {
				// 设置 z 字段
				"-zfield",
                _zfd,
                "-txe",
                extend[0].ToString(),extend[1].ToString(), // minx,maxx
                "-tye",
                extend[2].ToString(),extend[3].ToString(), // miny,maxy
                // 设置输出文件分辨率（以目标地理参考单位）,必须和 -txe 和 tye 一起使用才能生效
				"-outsize", innerstep.ToString(), innerstep.ToString(),
                "-a",
                "invdistnn:power=2.0:radius=100000.0:max_points=12:min_points=0:nodata=0"
                //"invdist:power=2.0:smoothing=0.0:radius1=0.0:radius2=0.0:angle=0.0:max_points=0:min_points=0:nodata=0.0"

		};
            GDALGridOptions gDALGridOptions = new GDALGridOptions(opt);
            var ds = Gdal.OpenEx(path + "\\" + layerName, 0, null, null, null);
            Dataset dks = Gdal.wrapper_GDALGrid(path + "\\" + tifName, ds, gDALGridOptions, null, string.Empty);

            dks.Dispose();
            ds.Dispose();

        }

        /// <summary>
        /// 栅格插值
        /// </summary>
        public void TestGdalGrid()
        {

            String path = @"Z:\jiayu\tmp\data\";
            // ▲注意一：参数 (-tr\-txe\-tye 需要一起使用),同时 前面参数不能和 tr 一起使用
            // ▲参数 a_srs 可以在这个位置找 http://epsg.io/4326 ，复制其中的 PROJ.4 
            string[] opt = new string[] {
				// 设置 z 字段
				"-zfield",
                "height",
				/*
				// 设置输出栅格的数据类型
				"-of",
				"Byte", // UInt16, Int16, UInt32, Int32, Float32, Float64, CInt16, CInt32, CFloat32, CFloat64
				// 设置输出栅格的 地理参考范围 (extent 的 minx,maxx,miny,maxy)
				"-txe",
				"1","100", // minx,maxx
				"-tye",
				"1","100", // miny,maxy
				// 设置输出文件分辨率（以目标地理参考单位）,必须和 -txe 和 tye 一起使用才能生效
				"-tr", "1", "1",
				// 设置输出文件的大小（以像素和行为单位）,不能和 -tr 一起使用
				"-outsize",
				"10","10",
				// 覆盖输出文件的投影。
				"-a_srs",
				"+proj=longlat +datum=WGS84 +no_defs",
				// 在 z 值的基础上增加一个数，例如原本 z 值是 1，增加 5 就变成了 6
				"-z_increase","5",
				// 同上，这里是 乘法 (1 + 5) * 6 = 36
				"-z_multiply","6",
				// 添加空间过滤器以仅选择包含在（xmin，ymin）-（xmax，ymax）描述的边界框中的要素。
				// 相当于设定 extent 的范围
				"-spat","0","0","1","1", // xmin ymin xmax ymax
				#region 这是一个参数集合，只有使用了 -clipsrc 才能使用后续的 clip 开头的其他参数
				// 添加空间过滤器以仅选择包含在指定边界框（在源SRS中表示），WKT几何（POLYGON或MULTIPOLYGON）中的要素，数据源或-spat选项的空间范围（如果使用spat_extent关键字）。 指定数据源时，通常将其与-clipsrclayer，-clipsrcwhere或-clipsrcsql选项结合使用。
				// 理解位 矢量 的 wkt 即可
				"-clipsrc","",
				// sql 表达式进行筛选(基于几何信息)
				"-clipsrcsql","不懂不知道怎么距离",
				// 选择图层
				"-clipsrclayer","layer名称",
				// sql 表达式进行筛选(基于字段)
				"-clipsrcwhere","字段<值",
				#endregion
				#region 对输入矢量本身而言的参数
				// 选择 数据集中的 图层（指的是输入矢量的图层）
				// 需要多个图层可以写多个例如
				// "-l","layer0","-l","layer1"
				"-l","layer0",
				// 对图层中的要素进行选择（sql和where我看不出差别，这里用一个例子）
				// 例子：https://gis.stackovernet.xyz/cn/q/43444
				// sql "select * from '图层名称'"
				// where "fid<5"
				"-where","",
				// 
				"-sql","",
				#endregion
				// 压缩格式选项
				// 这个参数用法大概如下
				// 1.指定输出文件例如 out.png 那么格式为 png
				// 2.到 https://gdal.org/drivers/raster/index.html#raster-drivers 下查找对应的格式的链接点击进去，
				//      例如 png 链接为 https://gdal.org/drivers/raster/png.html#raster-png
				// 3.最底下的 Creation Optins 都是可选项可以写多个，例如
				//      "-co","WORLDFILE=YES","-co","WRITE_METADATA_AS_TEXT=YES"
				"-co","看设计情况",
				// 抑制进度监视器和其他非错误输出。理解为屏蔽错误显示
				"-q",
				*/
				// 指定算法
				// 反向距离平方（默认）
				// invdist:power=2.0:smoothing=0.0:radius1=0.0:radius2=0.0:angle=0.0:max_points=0:min_points=0:nodata=0.0
				// 最近邻搜索到幂的反距离
				// invdistnn:power=2.0:radius=1.0:max_points=12:min_points=0:nodata=0
				// 移动平均
				// average:radius1=0.0:radius2=0.0:angle=0.0:min_points=0:nodata=0.0
				// 最近的邻居
				// nearest:radius1=0.0:radius2=0.0:angle=0.0:nodata=0.0
				// 各种数据指标
				// 指标可以是 minimum\maximum\range\count\average_distance\average_distance_pts
				// <metric name>:radius1=0.0:radius2=0.0:angle=0.0:min_points=0:nodata=0.0
				// <metric name> 可以是上面指标中的任一个，指标作用看这里 https://gdal.org/programs/gdal_grid.html#data-metrics
				// 例如 "-a","minimum:radius1=0.0:radius2=0.0:angle=0.0:min_points=0:nodata=0.0"
				// 
				// 线性
				// linear:radius=-1.0:nodata=0.0
				"-a", "invdist:power=2.0:smoothing=0.0:radius1=0.0:radius2=0.0:angle=0.0:max_points=0:min_points=0:nodata=0.0"


        };
            GDALGridOptions gDALGridOptions = new GDALGridOptions(opt);
            var ds = Gdal.OpenEx(path + "DrillPoints.shp", 0, null, null, null);
            Gdal.wrapper_GDALGrid(path + "out5.tif", ds, gDALGridOptions, null, string.Empty);
        }


        /// <summary>
        /// 读取栅格数据
        /// </summary>
        public static void ReadRaster(string strFile)
        {
            //注册
            Gdal.AllRegister();

            Dataset ds = Gdal.Open(strFile, Access.GA_ReadOnly);

            if (ds == null)
            {
                Console.WriteLine("不能打开：" + strFile);
                System.Environment.Exit(-1);
            }

            OSGeo.GDAL.Driver drv = ds.GetDriver();
            if (drv == null)
            {
                Console.WriteLine("不能打开：" + strFile);
                System.Environment.Exit(-1);
            }

            Console.WriteLine("RasterCount:" + ds.RasterCount);
            Console.WriteLine("RasterSize:" + ds.RasterXSize + " " + ds.RasterYSize);
        }

       
    }
}
