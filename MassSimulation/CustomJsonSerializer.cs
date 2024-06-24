﻿using MPI;
using System.Text.Json;

namespace MassSimulation
{
    /// <summary>
    /// This is a simple adapter that can be used in MPI.NET
    /// to serialize and deserialize objects.
    /// It is a replacement for the default serializer MPI.BinaryFormatterSerializer
    /// from MPI.NET, which uses the obsolete BinaryFormatter.
    /// </summary>
    internal class CustomJsonSerializer : ISerializer
    {
        public static readonly CustomJsonSerializer Default;

        static CustomJsonSerializer()
        {
            Default = new CustomJsonSerializer();
        }

        public T Deserialize<T>(Stream stream)
        {
            T? result = JsonSerializer.Deserialize<T>(stream);
            return result;
        }

        public void Serialize<T>(Stream stream, T value)
        {
            JsonSerializer.Serialize(stream, value);
        }
    }
}
