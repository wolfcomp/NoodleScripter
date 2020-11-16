using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NoodleScripter.Models.NoodleScripter;

namespace NoodleScripter.Models.BeatSaber
{
    public class EventData : ISerializable
    {
        [JsonProperty("_time")]
        public float Time { get; set; }
        [JsonProperty("_type")]
        public int Type { get; set; }
        [JsonProperty("_value")]
        public int Value { get; set; }
        [JsonProperty("_customData")]
        public EventCustomData CustomData { get; set; }

        public override string ToString()
        {
            return $"[time:{Time},type:{Type},value:{Value},custom:{CustomData.GetJsonString()}]";
        }
    }

    public class EventCustomData : ISerializable
    {
        [JsonProperty("_color")]
        public Color? Color { get; set; }
        [JsonProperty("_propID")]
        public int? PropID { get; set; }
        [JsonProperty("_lightID")]
        public int? LightID { get; set; }
    }

    public class LaserCustomData : EventCustomData
    {
        [JsonProperty("_lockPosition")]
        public bool? LockPosition { get; set; }
        [JsonProperty("_preciseSpeed")]
        public float? PreciseSpeed { get; set; }
        [JsonProperty("_direction")]
        public int? Direction { get; set; }
    }

    public class RingCustomData : EventCustomData
    {
        [JsonProperty("_nameFilter")]
        public string NameFilter { get; set; }
        [JsonProperty("_reset")]
        public bool? Reset { get; set; }
        [JsonProperty("_stepMult")]
        public float? StepMult { get; set; }
        [JsonProperty("_propMult")]
        public float? PropMult { get; set; }
        [JsonProperty("_speedMult")]
        public float? SpeedMult { get; set; }
        [JsonProperty("_speed")]
        public float? Speed { get; set; }
        [JsonProperty("_prop")]
        public float? Prop { get; set; }
        [JsonProperty("_step")]
        public float? Step { get; set; }
        [JsonProperty("_direction")]
        public int? Direction { get; set; }
        [JsonProperty("_counterSpin")]
        public bool? CounterSpin { get; set; }
    }
}
