using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASystem.Model
{
    public class TrainingInfo
    {
        /// <summary>
        /// トレーニング名。トレーニング選択画面に表示される。
        /// </summary>
        [JsonProperty("trainingName")]
        public String TrainingName { get; set; }

        /// <summary>
        /// 固定する関節を管理するリスト
        /// </summary>
        [JsonProperty("fixTrackingTargets")]
        public List<FixTrackingTarget> FixTrackingTargets { get; set; }

        /// <summary>
        /// 角度範囲の追跡対象を管理するリスト
        /// </summary>
        [JsonProperty("rangeTrackingTargets")]
        public List<RangeTrackingTarget> RangeTrackingTargets { get; set; }


    }
}
