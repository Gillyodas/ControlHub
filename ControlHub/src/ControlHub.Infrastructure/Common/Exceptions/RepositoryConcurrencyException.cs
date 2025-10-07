namespace ControlHub.Infrastructure.Common.Exceptions
{
    public class RepositoryConcurrencyException : RepositoryException
    {
        public RepositoryConcurrencyException(string message, Exception? inner = null)
            : base(message, inner) { }
    }
}
