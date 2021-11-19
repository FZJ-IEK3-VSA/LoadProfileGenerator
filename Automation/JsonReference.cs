using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Automation {

    /// <summary>
    /// A class for creating JsonReference objects from Json. This is used so that simple name strings
    /// can be passed alternatively to complete JsonReference objects with name and Guid.
    /// </summary>
    class JsonReferenceConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(JsonReference));
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            switch (token.Type) {
                case JTokenType.Null:
                    return null;
                case JTokenType.String:
                     // if the json only contains a string, then use this as the name and leave the Guid blank
                    string? name = (string?) token;
                    return new JsonReference(name!, StrGuid.Empty);
                default:
                    JsonReference? jsonReference = new JsonReference();
                    serializer.Populate(token.CreateReader(), jsonReference);
                    return jsonReference;
            }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(JsonReferenceConverter))]
    public class JsonReference : IGuidObject, IEquatable<JsonReference>
    {
        [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public bool Equals(JsonReference other)
            => Name == other?.Name && Guid == other?.Guid;

        public static bool operator ==(JsonReference? point1, JsonReference? point2)
        {
            if (point1 is null && point2 is null)
            {
                return true;
            }

            if (point1 is null)
            {
                return false;
            }
            if (point2 is null)
            {
                return false;
            }

            return point1.Equals(point2);
        }

        public static bool operator !=(JsonReference? point1, JsonReference? point2)
        {
            if (point1 is null && point2 is null) {
                return false;
            }

            if (point1 is null ) {
                return true;
            }
            if (point2 is null)
            {
                return true;
            }
            return !point1.Equals(point2);
        }

        public override bool Equals(object obj) => obj is JsonReference other && Equals(other);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked {
                return (Name?.GetHashCode()??0 * 397) ^ (Guid.GetHashCode());
            }
        }

        public JsonReference([JetBrains.Annotations.NotNull] string name, StrGuid guid)
        {
            Name = name;
            Guid = guid;
        }


        /// <summary>
        /// for json
        /// </summary>
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public JsonReference()
        {
            //Guid = StrGuid.Empty;
        }

        [JetBrains.Annotations.NotNull]
        public string? Name { get; set; }
        public StrGuid Guid { get; set; }

        [JetBrains.Annotations.NotNull]
        public override string ToString() => Name + "(" + Guid + ")";
    }
}