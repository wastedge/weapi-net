using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WastedgeApi
{
    public class ApiException : Exception
    {
        private ApiRowErrorsCollection _errors;

        public ApiRowErrorsCollection Errors
        {
            get
            {
                if (_errors == null)
                    _errors = new ApiRowErrorsCollection();
                return _errors;
            }
        }

        public string CallStack { get; }

        public ApiException()
        {
        }

        public ApiException(string message)
            : base(message)
        {
        }

        public ApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ApiException(string message, string callStack, Exception innerException)
            : base(message, innerException)
        {
            CallStack = callStack;
        }

        public ApiException(ApiRowErrorsCollection errors)
        {
            _errors = errors;
        }

        public ApiException(string message, ApiRowErrorsCollection errors)
            : base(message)
        {
            _errors = errors;
        }

        public ApiException(string message, Exception innerException, ApiRowErrorsCollection errors)
            : base(message, innerException)
        {
            _errors = errors;
        }
    }
}
