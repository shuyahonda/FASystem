using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    public class AngleRange
    {
        [JsonProperty("start")]
        public float Start { get; set; }

        [JsonProperty("end")]
        public float End { get; set; }
    }
}
