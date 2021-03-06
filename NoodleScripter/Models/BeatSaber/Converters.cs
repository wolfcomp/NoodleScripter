﻿using System;
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
            writer.WriteValue(value.R);
            writer.WriteValue(value.G);
            writer.WriteValue(value.B);
            writer.WriteValue(value.A);
            writer.WriteEndArray();
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            return Color.FromArgb(array.Count == 4 ? array[3].ToObject<double>() : 1, array[0].ToObject<double>(), array[1].ToObject<double>(), array[2].ToObject<double>());
        }
    }

    public class VectorConverter : JsonConverter<Vector>
    {
        public override void WriteJson(JsonWriter writer, Vector value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(Math.Round(value.X, 4));
            writer.WriteValue(Math.Round(value.Y, 4));
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
            writer.WriteValue(Math.Round(value.X, 4));
            writer.WriteValue(Math.Round(value.Y, 4));
            writer.WriteValue(Math.Round(value.Z, 4));
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
            if (!value.HasValue) return;
            writer.WriteStartArray();
            writer.WriteValue(Math.Round(value.Value.X, 4));
            writer.WriteValue(Math.Round(value.Value.Y, 4));
            writer.WriteValue(Math.Round(value.Value.Z, 4));
            writer.WriteEndArray();
        }

        public override Vector3D? ReadJson(JsonReader reader, Type objectType, Vector3D? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }
}
