﻿#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.Core.Numerics;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Numerics.Converters
{
    internal class JsonConverterUInt512 : JsonConverter<UInt512>
    {
        public override UInt512 ReadJson(JsonReader reader, Type objectType, UInt512 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = (string)reader.Value;
            return UInt512.FromHex(s);
        }

        public override void WriteJson(JsonWriter writer, UInt512 value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}