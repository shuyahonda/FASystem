using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    class TrainingInfo
    {
        [JsonProperty("trainingName")]
        public String TrainingName { get; set; }

        [JsonProperty("fixTrackingTargets")]
        public List<FixTrackingTarget> FixTrackingTargets { get; set; }

        [JsonProperty("rangeTrackingTargets")]
        public List<RangeTrackingTarget> RangeTrackingTargets { get; set; }

        public TrainingInfo()
        {

        }

    }
}
