using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;

namespace NoodleScripter.Models.BeatSaber
{
    public class ObjectData : ISerializable
    {
        [JsonProperty("_time")]
        public float Time { get; set; }
        [JsonProperty("_lineIndex")]
        public int LineIndex { get; set; }
        [JsonProperty("_type")]
        public int Type { get; set; }
        [JsonIgnore]
        public bool Note { get; set; }
        [JsonProperty("_customData")]
        public ObjectCustom CustomData { get; set; }
    }

    public class ObjectBomb : ObjectData
    {
        [JsonProperty("_lineLayer")]
        public int LineLayer { get; set; }
        [JsonProperty("_cutDirection")]
        public int CutDirection { get; set; }
    }

    public class ObjectWall : ObjectData
    {
        
        [JsonProperty("_duration")]
        public float Duration { get; set; }
        [JsonProperty("_width")]
        public int Width { get; set; }
    }

    public class ObjectCustom
    {
        [JsonProperty("_position")]
        public Vector Position { get; set; }
        [JsonProperty("_color")]
        public Vector3D? Color { get; set; }
        [JsonProperty("_localRotation")]
        public Vector3D LocalRotation { get; set; }
        [JsonProperty("_rotation")]
        public Vector3D Rotation { get; set; }
        [JsonProperty("_track")]
        public string Track { get; set; }
        [JsonProperty("_noteJumpMovementSpeed")]
        public double? NJS { get; set; }
        [JsonProperty("_noteJumpStartBeatOffset")]
        public double? Offset { get; set; }
    }

    public class ObstacleCustom : ObjectCustom
    {
        [JsonProperty("_scale")]
        public Vector Scale { get; set; }
    }

    public class NoteCustom : ObjectCustom
    {
        [JsonProperty("_cutDirection")]
        public int CutDirection { get; set; }
        [JsonProperty("_flip")]
        public Vector Flip { get; set; }
        [JsonProperty("_disableSpawnEffect")]
        public bool DisableSpawnEffect { get; set; }
        [JsonProperty("_fake")]
        public bool Fake { get; set; }
        [JsonProperty("_interactable")]
        public bool Interactable { get; set; }
        [JsonProperty("_animation")]
        public object Animation { get; set; }
    }
}
