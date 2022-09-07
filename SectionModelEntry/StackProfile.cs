using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanyonRecoveryEntry
{
   public static  class StackProfile
    {
        /// <summary>
        /// 创建堆栈剖面
        /// </summary>
        /// <param name="_3DControlLine"></param>
        public static void CreateStackProfile(Geometry _3DControlLine, string _savePath)
        {
            Dictionary<double, double> profileValue = new Dictionary<double, double>();

            double distance = 0.0;
            for (int i = 0; i < _3DControlLine.GetPointCount(); i++)
            {
                if (i == 0)
                {
                    distance = distance + 0.0;

                    profileValue.Add(distance, _3DControlLine.GetZ(i));
                }
                else
                {
                    Geometry pt1 = new Geometry(wkbGeometryType.wkbPoint);
                    pt1.AddPoint_2D(_3DControlLine.GetX(i), _3DControlLine.GetY(i));

                    Geometry pt2 = new Geometry(wkbGeometryType.wkbPoint);
                    pt2.AddPoint_2D(_3DControlLine.GetX(i - 1), _3DControlLine.GetY(i - 1));

                    distance = distance + pt1.Distance(pt2);

                    profileValue.Add(distance, _3DControlLine.GetZ(i));
                }
            }

            //将信息输出
            string subFileName = _savePath + "\\" + "profile2.txt";
            export2txt(subFileName, profileValue);
        }


        /// <summary>
        /// 将剖面线信息写入到txt中，
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="_angleDic"></param>
        public static void export2txt(string fileName, Dictionary<double, double> _angleDic)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    //sw.WriteLine("o Head");
                    //sw.WriteLine("g default");

                    string tl = string.Empty;

                    foreach (var key in _angleDic.Keys)
                    {
                        tl = Convert.ToString(key) + "," + Convert.ToString(_angleDic[key]);
                        sw.WriteLine(tl);
                    }
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("写入错误！！！");
            }
        }
    }
}
