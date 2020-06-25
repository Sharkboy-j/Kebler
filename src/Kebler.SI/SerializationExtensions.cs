using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Kebler.SI
{
    internal static class SerializationExtensions
    {
        private static readonly BinaryFormatter Formatter = new BinaryFormatter();

        internal static byte[] Serialize<T>(this T obj)
        {
            using var memoryStream = new MemoryStream();
            Formatter.Serialize(memoryStream, obj);
            return memoryStream.ToArray();
        }

        internal static T Deserialize<T>(this byte[] data)
        {
            using var memoryStream = new MemoryStream(data);
            var obj = Formatter.Deserialize(memoryStream);
            return (T)obj;
        }
    }
}
