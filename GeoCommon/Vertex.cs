using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeoCommon
{
   public class Vertex
    {
        /// <summary>
        /// 顶点的id
        /// </summary>
        public int id;

        /// <summary>
        /// 顶点的X坐标
        /// </summary>
        public double x;

        /// <summary>
        /// 顶点的Y坐标
        /// </summary>
        public double y;

        /// <summary>
        /// 顶点的Z坐标
        /// </summary>
        public double z;

        /// <summary>
        /// 所属三角形Id列表
        /// </summary>
        public ArrayList triangleIds = new ArrayList();

        public Vertex(double x = double.NaN, double y = double.NaN, double z = double.NaN)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 重载减号
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vertex operator -(Vertex a, Vertex b)
        {
            return new Vertex(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        /// <summary>
        /// 重载加号
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vertex operator +(Vertex a, Vertex b)
        {
            return new Vertex(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        /// <summary>
        /// 重载乘号
        /// </summary>
        /// <param name="k"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vertex operator *(double k, Vertex a)
        {
            return new Vertex(a.x * k, a.y * k, a.z * k);
        }
        /// <summary>
        /// 重载除号
        /// </summary>
        /// <param name="a"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static Vertex operator /(Vertex a, double k)
        {
            return new Vertex(a.x / k, a.y / k, a.z / k);
        }
        /// <summary>
        /// 计算单位向量
        /// </summary>
        /// <param name="a">输入向量</param>
        /// <returns>单位向量</returns>
        public void Normal()
        {
            if (x * y * z != 0)
            {
                double length = Math.Sqrt(x * x + y * y + z * z);///计算向量的模
                x /= length;
                y /= length;
                z /= length;
            }
        }
        /// <summary>
        /// 计算向量叉积
        /// </summary>
        /// <param name="a">向量a</param>
        /// <param name="b">向量b</param>
        /// <returns>叉积向量</returns>
        public static Vertex CrossProduct(Vertex a, Vertex b)
        {
            double x = a.y * b.z - b.y * a.z;// y1z2-y2z1,z1x2-z2x1,x1y2-x2y1
            double y = a.z * b.x - a.x * b.z;
            double z = a.x * b.y - b.x * a.y;
            return new Vertex(x, y, z);
        }
        /// <summary>
        /// 计算空间向量点积
        /// </summary>
        /// <param name="a">向量a</param>
        /// <param name="b">向量b</param>
        /// <returns>向量积</returns>
        public static double Dot(Vertex a, Vertex b)
        {
            return (a.x * b.x + a.y * b.y + a.z * b.z);
        }
    }
}
