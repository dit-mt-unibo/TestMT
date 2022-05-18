namespace TestMT.ModernMT
{
    public class ApiConnectionException : ApiException
    {
        private const string DefaultMessage = "Unable to connect to server, please verify your internet connection and retry.";

        public ApiConnectionException(string message) : base(0, "ConnectionException", message)
        {
        }

        public ApiConnectionException() : this(DefaultMessage)
        {
        }

        public ApiConnectionException(string message, System.Exception innerException) : base(0, "ConnectionException", message, innerException)
        {
        }

        public ApiConnectionException(System.Exception innerException) : this(DefaultMessage, innerException)
        {
        }
    }
}