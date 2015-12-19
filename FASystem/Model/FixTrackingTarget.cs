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
    class FixTrackingTarget
    {
        [JsonProperty("fixType")]
        public FixType FixType;

        [JsonProperty("targetJoint")]
        public JointType TargetJoint;

        [JsonProperty("permissibleRange")]
        public float PermissibleRange { get; set; }
    }
}
