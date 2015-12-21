using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    /// <summary>
    /// グラフの点を表すクラス
    /// </summary>
    public class GraphPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public GraphPoint(int X,int Y)
        {
            this.X = X;
            this.Y = Y;
        }

    }
}
