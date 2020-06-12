namespace Transmission.API.RPC.Entity
{
    public class EntityBase<T>
    {
        public Enums.ReponseResult Result { get; set; }
        public T Value { get; set; }
    }
}
