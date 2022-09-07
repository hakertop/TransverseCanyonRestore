using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeoCommon
{
    /// <summary>
    /// 二维节点
    /// </summary>
    public class Vertex2D
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string name;

        /// <summary>
        /// 隶属于那个Eage
        /// </summary>
        public string belongTo;

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


        public Vertex2D()
        {

        }

        public Vertex2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
