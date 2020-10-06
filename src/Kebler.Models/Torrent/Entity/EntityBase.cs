namespace Kebler.Models.Torrent.Entity
{
    public class EntityBase<T>
    {
        public Enums.ReponseResult Result { get; set; }
        public T Value { get; set; }
    }
}