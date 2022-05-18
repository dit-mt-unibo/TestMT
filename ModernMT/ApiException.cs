using System;

namespace TestMT.ModernMT
{
    public class ApiException : Exception
    {
        public static readonly int InvalidAccessTokenCode = 401;

        public readonly int Code;
        public readonly string Type;

        public static ApiException UnexpectedException(Exception innerException)
        {
            return new ApiException(500, "UnknownException", innerException.Message, innerException);
        }

        public static ApiException FromJson(dynamic json)
        {
            int code = json.status;
            string type = "UnknownException";
            string message = "No details provided.";

            dynamic error = json.error;
            if (error != null)
            {
                if (error.type != null)
                    type = error.type;
                if (error.message != null)
                    message = error.message;
            }

            return new ApiException(code, type, message);
        }

        public ApiException(int code, string type, string message) : base(message)
        {
            Code = code;
            Type = type;
        }

        public ApiException(int code, string type, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
            Type = type;
        }

        public override string ToString()
        {
            return Message + " (" + Code + " - " + Type + ")";
        }

    }
}