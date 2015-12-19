using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    public class Tempo
    {
        [JsonProperty("restTimeInBottom")]
        public float RestTimeInBottom{ get; set; }

        [JsonProperty("restTimeInTop")]
        public float RestTimeInTop { get; set; }

        [JsonProperty("downwardMovementTime")]
        public float DownwardMovementTime { get; set; }

        [JsonProperty("riseMomentTime")]
        public float RiseMomementTime { get; set; } 
    }
}
