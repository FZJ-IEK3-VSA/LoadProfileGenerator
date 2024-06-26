using System.Text.Json;

namespace MassSimulation
{
    /// <summary>
    /// This is a simple adapter that can be used in MPI.NET
    /// to serialize and deserialize objects.
    /// It is a replacement for the default serializer MPI.BinaryFormatterSerializer
    /// from MPI.NET, which uses the obsolete BinaryFormatter.
    /// </summary>
    internal class MPIJsonSerializer : MPI.ISerializer
    {
        public static readonly MPIJsonSerializer Default;

        static MPIJsonSerializer()
        {
            Default = new MPIJsonSerializer();
        }

        public T? Deserialize<T>(Stream stream)
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
