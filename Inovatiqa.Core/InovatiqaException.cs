using System;
using System.Runtime.Serialization;

namespace Inovatiqa.Core
{
    [Serializable]
    public class InovatiqaException : Exception
    {
        public InovatiqaException()
        {
        }

        public InovatiqaException(string message)
            : base(message)
        {
        }

        public InovatiqaException(string messageFormat, params object[] args)
            : base(string.Format(messageFormat, args))
        {
        }

        protected InovatiqaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public InovatiqaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
