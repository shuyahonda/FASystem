using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    public class GraphPoint
    {
        public float X { get; set; }
        public float Y { get; set; }

        public GraphPoint(float X,float Y)
        {
            this.X = X;
            this.Y = Y;
        }

    }
}
