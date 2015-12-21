using FASystem.Enum;
using Microsoft.Kinect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    public class RangeTrackingTarget
    {
        private const int kinectFPS = 30;

        public Boolean isUserUnitVector { get; set; }
        public Boolean isManageTempo { get; set; }
   
        [JsonProperty("tempo")]
        public Tempo Tempo { get; set; }

        [JsonProperty("permissibleRangeInTop")]
        public AngleRange PermissibleRangeInTop { get; set; }

        [JsonProperty("permissibleRangeInBottom")]
        public AngleRange PermissibleRangeInBottom { get; set; }

        [JsonProperty("prohibitedRangeInTop")]
        public AngleRange ProhibitedRangeInTop { get; set; }

        [JsonProperty("prohibitedRangeInBottom")]
        public AngleRange ProhibitedRangeInBottom { get; set; }

        [JsonProperty("planeType")]
        public PlaneType PlaneType { get; set; }

        [JsonProperty("origin")]
        public JointType Origin { get; set; }

        [JsonProperty("vector")]
        public List<JointType> Vector { get; set; }




        public float getAllTime()
        {
            return Tempo.RestTimeInBottom + Tempo.RestTimeInTop + Tempo.DownwardMovementTime + Tempo.RiseMovementTime;
        }

        public float getXRange()
        {
            return getAllTime() * kinectFPS;
        }

        //TODO: このメソッドは要修正
        public ObservableCollection<GraphPoint> generateBindingGraphCollection()
        {
            ObservableCollection<GraphPoint> collection = new ObservableCollection<GraphPoint>();

            int XRange = (int)this.getXRange();

            for (int x = 0; x < XRange; x++)
            {
                float restTimeInBottom = this.Tempo.RestTimeInBottom;
                float restTimeInTop = this.Tempo.RestTimeInTop;
                float y = 0;

                if (x < restTimeInBottom * kinectFPS)
                {
                    // 下降時の休憩 180
                    y = PermissibleRangeInBottom.calcAverage() ;
                } else if(x >= restTimeInBottom * kinectFPS && x <= (restTimeInBottom + Tempo.RiseMovementTime) * kinectFPS)
                {
                    // 上昇中　180 -> 30
                    float downRange = (PermissibleRangeInBottom.calcAverage() - PermissibleRangeInTop.calcAverage()) / Tempo.RiseMovementTime / kinectFPS;
                    Console.WriteLine("downRange" + downRange);
                    y = PermissibleRangeInBottom.calcAverage() - downRange * (x - restTimeInBottom * kinectFPS);
                } else if(x >= (restTimeInBottom + Tempo.RiseMovementTime) * kinectFPS && x <= (restTimeInBottom + Tempo.RiseMovementTime + restTimeInTop) * kinectFPS)
                {
                    // 上昇時の休憩 30
                    y = PermissibleRangeInTop.calcAverage();
                } else
                {
                    // 上昇からの下降 30 -> 180
                    float upRange = (PermissibleRangeInBottom.calcAverage() - PermissibleRangeInTop.calcAverage()) / Tempo.DownwardMovementTime / kinectFPS;
                    Console.WriteLine("upRange" + upRange);
                    y = PermissibleRangeInTop.calcAverage() + upRange * (x - (restTimeInTop + restTimeInBottom + Tempo.RiseMovementTime) * kinectFPS);
                }
                
                

                Console.WriteLine("Add Point = " + x + "," + y);
                GraphPoint point = new GraphPoint(x,(int)y);
                collection.Add(point);
            }

            return collection;
        }
    }
}
