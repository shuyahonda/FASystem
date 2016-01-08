using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Helper
{
    public static class Utility
    {
        /// <summary>
        /// ラジアンから度に変換する
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double radToDegree(double num)
        {
            return num * 180 / Math.PI;
        }
    }
}
