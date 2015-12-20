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
            return Tempo.RestTimeInBottom + Tempo.RestTimeInTop + Tempo.DownwardMovementTime + Tempo.RiseMomementTime;
        }

        public float getXRange()
        {
            return getAllTime() * kinectFPS;
        }

        public ObservableCollection<GraphPoint> generateBindingGraphCollection()
        {
            ObservableCollection<GraphPoint> collection = new ObservableCollection<GraphPoint>();

            int XRange = (int)this.getXRange();

            for (int x = 0; x < XRange; x++)
            {
                float restTimeInBottom = this.Tempo.RestTimeInBottom;
                float restTimeInTop = this.Tempo.RestTimeInTop;
                float y = 0;

                if (x <= restTimeInBottom)
                {
                    y = restTimeInBottom;
                } else
                { 

                    float upRange = Tempo.RiseMomementTime / (ProhibitedRangeInTop.calcAverage() - PermissibleRangeInBottom.calcAverage());
                    y = restTimeInBottom + upRange * (x - restTimeInBottom);
                }

                GraphPoint point = new GraphPoint(x,y);
                collection.Add(point);
            }

            return collection;
        }
    }
}
