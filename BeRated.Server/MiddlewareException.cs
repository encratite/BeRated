using System;

namespace BeRated.Server
{
    class MiddlewareException : Exception
    {
        public MiddlewareException(string message)
            : base(message)
        {
        }
    }
}
