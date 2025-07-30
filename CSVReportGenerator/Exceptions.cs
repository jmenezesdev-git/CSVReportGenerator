
internal class UnknownErrorException : Exception
{
    public UnknownErrorException()
    {
    }

    public UnknownErrorException(string? message) : base(message)
    {
    }

    public UnknownErrorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}