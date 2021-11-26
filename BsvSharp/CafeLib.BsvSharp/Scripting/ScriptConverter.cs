#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Scripting
{
    internal class ScriptConverter : JsonConverter<Script>
    {
        public override Script ReadJson(JsonReader reader, Type objectType, Script existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = (string)reader.Value;
            return new Script(s);
        }

        public override void WriteJson(JsonWriter writer, Script value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToHexString());
        }
    }
}
