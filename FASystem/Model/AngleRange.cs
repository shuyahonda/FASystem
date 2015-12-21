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


        /// <summary>
        /// 角度範囲の平均値を返す
        /// </summary>
        /// <returns></returns>
        public float calcAverage()
        {
            return (Start + End) / 2;
        }

        /// <summary>
        /// 角度範囲の値を返す
        /// </summary>
        /// <returns></returns>
        public float getRange()
        {
            return Start > End ? Start - End : End - Start;
        }
    }
}
