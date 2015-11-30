using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    class TrainingInfo
    {
        public String TrainingName { get; set; }
        public List<FixTrackingTarget> FixTrackingTargets { get; set; }
        List<RangeTrackingTarget> RangeTrackingTargets { get; set; }

        public TrainingInfo()
        {

        }

    }
}
