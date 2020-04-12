using System;

namespace MediaServer.Core.Errors
{
    sealed class OperationForbiddenException : Exception
    {
        public OperationForbiddenException(string message) : base(message)
        {
        }
    }
}
