using FASystem.Helper;
using Newtonsoft.Json;
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
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("restTimeInBottom")]
        public float RestTimeInBottom{ get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("restTimeInTop")]
        public float RestTimeInTop { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("downwardMovementTime")]
        public float DownwardMovementTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("riseMovementTime")]
        public float RiseMovementTime { get; set; } 

        /// <summary>
        /// 動作開始から元に戻るまでにかかる時間を返す
        /// </summary>
        /// <returns></returns>
        public float getAllTime()
        {
            return RestTimeInBottom + RestTimeInTop + DownwardMovementTime + RiseMovementTime;
        }

        /// <summary>
        /// 動作開始から元に戻るまでに必要な総フレーム数
        /// Kinectのフレーム数は30
        /// </summary>
        /// <returns></returns>
        public float getAllFrame()
        {
            return this.getAllTime() * CommonConstants.KINECT_FPS;
        }
    }
}
