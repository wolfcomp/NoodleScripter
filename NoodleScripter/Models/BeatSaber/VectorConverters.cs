using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NoodleScripter.Models.BeatSaber
{
    public class VectorConverter : JsonConverter<Vector>
    {
        public override void WriteJson(JsonWriter writer, Vector value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.X);
            writer.WriteValue(value.Y);
            writer.WriteEndArray();
        }

        public override Vector ReadJson(JsonReader reader, Type objectType, Vector existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }

    public class Vector3DConverter : JsonConverter<Vector3D>
    {
        public override void WriteJson(JsonWriter writer, Vector3D value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.X);
            writer.WriteValue(value.Y);
            writer.WriteValue(value.Z);
            writer.WriteEndArray();
        }

        public override Vector3D ReadJson(JsonReader reader, Type objectType, Vector3D existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }

    public class NullableVector3DConverter : JsonConverter<Vector3D?>
    {
        public override void WriteJson(JsonWriter writer, Vector3D? value, JsonSerializer serializer)
        {
            if(!value.HasValue) return;
            writer.WriteStartArray();
            writer.WriteValue(value.Value.X);
            writer.WriteValue(value.Value.Y);
            writer.WriteValue(value.Value.Z);
            writer.WriteEndArray();
        }

        public override Vector3D? ReadJson(JsonReader reader, Type objectType, Vector3D? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }
}
