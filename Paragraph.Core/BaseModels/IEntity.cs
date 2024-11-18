namespace Paragraph.Core.BaseModels
{
    public interface IEntity<T>
    {
        public T Id { get; set; }
    }
}