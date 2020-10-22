using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoodleScripter.Models.NoodleScripter;

namespace NoodleScripter.Models.BeatSaber
{
    public class ColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue((float)value.R);
            writer.WriteValue((float)value.G);
            writer.WriteValue((float)value.B);
            writer.WriteValue((float)value.A);
            writer.WriteEndArray();
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }

    public class VectorConverter : JsonConverter<Vector>
    {
        public override void WriteJson(JsonWriter writer, Vector value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue((float)value.X);
            writer.WriteValue((float)value.Y);
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
            writer.WriteValue((float)value.X);
            writer.WriteValue((float)value.Y);
            writer.WriteValue((float)value.Z);
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
            writer.WriteValue((float)value.Value.X);
            writer.WriteValue((float)value.Value.Y);
            writer.WriteValue((float)value.Value.Z);
            writer.WriteEndArray();
        }

        public override Vector3D? ReadJson(JsonReader reader, Type objectType, Vector3D? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }
}
