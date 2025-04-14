namespace Kelimedya.Core.BaseModels
{
    public interface IEntity<T>
    {
        public T Id { get; set; }
    }
}