using FASystem.Enum;
using Microsoft.Kinect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    public class RangeTrackingTarget
    {
        public Boolean isUserUnitVector { get; set; }
        public Boolean isManageTempo { get; set; }
   
        [JsonProperty("tempo")]
        public Tempo Tempo { get; set; }

        [JsonProperty("permissibleRange")]
        public AngleRange PermissibleRange { get; set; }

        [JsonProperty("prohibitedRange")]
        public AngleRange ProhibitedRange { get; set; }

        [JsonProperty("planeType")]
        public PlaneType PlaneType { get; set; }

        [JsonProperty("origin")]
        public JointType Origin { get; set; }

        [JsonProperty("vector")]
        public List<JointType> Vector { get; set; }
    }
}
