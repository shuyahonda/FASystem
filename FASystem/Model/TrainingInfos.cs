using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    class TrainingInfos
    {
        [JsonProperty("trainingInfos")]
        public List<TrainingInfo> infos { get; set; }
    }
}
