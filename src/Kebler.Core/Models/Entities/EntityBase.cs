using Kebler.TransmissionTorrentClient.Models;

namespace Kebler.Core.Models.Entities
{
    public class EntityBase<T>
    {
        public Enums.ReponseResult Result { get; set; }

        public T Value { get; set; }
    }
}