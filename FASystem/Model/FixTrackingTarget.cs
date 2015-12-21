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
    public class FixTrackingTarget
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("fixType")]
        public FixType FixType;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("targetJoint")]
        public JointType TargetJoint;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("permissibleRange")]
        public float PermissibleRange { get; set; }
    }
}
