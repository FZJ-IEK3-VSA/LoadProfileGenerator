using Automation.ResultFiles;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automation {
    /// <summary>
    /// A class for creating JsonReference objects from Json with System.Text.Json.
    /// This is used so that simple name strings can be passed alternatively to
    /// complete JsonReference objects with name and Guid.
    /// </summary>
    public class JsonReferenceConverter : JsonConverter<JsonReference>
    {
        public override JsonReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                // If it's a string, create a JsonReference with the name set to the string value
                return new JsonReference(reader.GetString()!);
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                // If it's an object, deserialize it normally
                var newOptions = new JsonSerializerOptions(options);
                // remove this converter from the list to avoid recursion
                newOptions.Converters.Remove(this);
                var jsonReference = JsonSerializer.Deserialize<JsonReference>(ref reader, newOptions)!;
                return jsonReference;
            }
            else
            {
                throw new JsonException("Unexpected JSON format for JsonReference.");
            }
        }

        public override void Write(Utf8JsonWriter writer, JsonReference value, JsonSerializerOptions options)
        {
            if ((value.Guid == null || value.Guid == StrGuid.Empty) && !string.IsNullOrEmpty(value.Name))
            {
                // If only the name is set, serialize it as a string
                writer.WriteStringValue(value.Name);
            }
            else
            {
                // Otherwise, serialize it normally as an object
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }

    /// <summary>
    /// A class for creating JsonReference objects from Json with Newtonsoft.
    /// This is used so that simple name strings can be passed alternatively to
    /// complete JsonReference objects with name and Guid.
    /// </summary>
    class JsonReferenceConverterNewtonsoft : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(JsonReference));
        }

        public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            Newtonsoft.Json.Linq.JToken token = Newtonsoft.Json.Linq.JToken.Load(reader);
            switch (token.Type) {
                case Newtonsoft.Json.Linq.JTokenType.Null:
                    return null;
                case Newtonsoft.Json.Linq.JTokenType.String:
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

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    [Newtonsoft.Json.JsonConverter(typeof(JsonReferenceConverterNewtonsoft))]
    public class JsonReference : IGuidObject, IEquatable<JsonReference>
    {
        [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public bool Equals(JsonReference? other)
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

        public override bool Equals(object? obj) => obj is JsonReference other && Equals(other);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked {
                return (Name?.GetHashCode()??0 * 397) ^ (Guid.GetHashCode());
            }
        }

        public JsonReference([JetBrains.Annotations.NotNull] string name, StrGuid? guid = null)
        {
            Name = name;
            Guid = guid ?? StrGuid.Empty;
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