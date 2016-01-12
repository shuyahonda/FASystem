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

        /// <summary>
        /// 角度が範囲内にあるか判定する
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public bool inRangeAngle(int angle)
        {
            if (angle <= getLargeAngle() && angle >= getSmallAngle())
            {
                return true;
            } else
            {
                return false;
            }
        }

        private float getSmallAngle()
        {
            return Start > End ? End : Start;
        }

        private float getLargeAngle()
        {
            return Start > End ? Start : End;
        }
    }
}
