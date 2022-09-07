using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeoCommon
{
    public class Triangle
    {
        /// <summary>
        /// 三角形ID
        /// </summary>
        public int id;

        /// <summary>
        /// 顶点0ID
        /// </summary>
        public int v0;

        /// <summary>
        /// 顶点1ID
        /// </summary>
        public int v1;

        /// <summary>
        /// 顶点2ID
        /// </summary>
        public int v2;

        /// <summary>
        /// 所属triMeshName的ID
        /// </summary>
        public int triMeshId;

        /// <summary>
        /// 三角面片类型
        /// </summary>
        public TriangleType type = TriangleType.Unknown;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Triangle()
        {
        }
    }
    

    /// <summary>
    /// 三角面片类型
    /// </summary>
    public enum TriangleType
    {
        Unknown,
        Normal,
        Terrian,
        Bundary
    }
}

