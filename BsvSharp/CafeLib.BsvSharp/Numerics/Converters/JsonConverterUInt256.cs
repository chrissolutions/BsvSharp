#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.Core.Numerics;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Numerics.Converters
{
    internal class JsonConverterUInt256 : JsonConverter<UInt256>
    {
        public override UInt256 ReadJson(JsonReader reader, Type objectType, UInt256 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = (string)reader.Value;
            return UInt256.FromHex(s);
        }

        public override void WriteJson(JsonWriter writer, UInt256 value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}