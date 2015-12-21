﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        [JsonProperty("riseMovementTime")]
        public float RiseMovementTime { get; set; } 

    }
}
