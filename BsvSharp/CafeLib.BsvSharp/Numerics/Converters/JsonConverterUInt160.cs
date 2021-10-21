using System;
using CafeLib.Core.Numerics;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Numerics.Converters
{
    internal class JsonConverterUInt160 : JsonConverter<UInt160>
    {
        public override UInt160 ReadJson(JsonReader reader, Type objectType, UInt160 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = (string)reader.Value;
            return new UInt160(s);
        }

        public override void WriteJson(JsonWriter writer, UInt160 value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}