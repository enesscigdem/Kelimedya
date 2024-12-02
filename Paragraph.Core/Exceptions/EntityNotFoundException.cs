namespace Paragraph.Core.Exceptions
{
    [Serializable]
    public class EntityNotFoundException : Exception
    {
        private int key;

        public EntityNotFoundException()
        {
        }

        public EntityNotFoundException(string message) : base(message)
        {
        }
        public EntityNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
        public EntityNotFoundException(string entityName, int key) : this(
            $"Specified entity can not found. Entity: {entityName} PrimaryKay: {key}")
        {
            this.key = key;
        }

        protected EntityNotFoundException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}