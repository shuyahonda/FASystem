using FASystem.Enum;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    class FixTrackingTarget
    {
        public FixType FixType;
        public JointType TargetJoint;
        public float PermissibleRange { get; set; }
    }
}
