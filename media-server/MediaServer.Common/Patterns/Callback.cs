using System;
using System.Threading;

namespace MediaServer.Common.Patterns
{
    public interface ICallback
    {
        void Error(string errorMessage);
    }

    public sealed class CallbackAlreadyCalledException : System.Exception { }

    public sealed class Callback<TResult> : ICallback
    {
        Action<TResult> _resultHandler;
        Action<string> _errorHandler;
        int _called;

        public void Result(TResult result)
        {
            if(Interlocked.CompareExchange(ref _called, 1, 0) == 1)
            {
                throw new CallbackAlreadyCalledException();
            }
            _resultHandler?.Invoke(result);
        }

        public void Error(string errorMessage)
        {
            if(Interlocked.CompareExchange(ref _called, 1, 0) == 1)
            {
                throw new CallbackAlreadyCalledException();
            }
            _errorHandler?.Invoke(errorMessage);
        }

        public Callback<TResult> OnResult(Action<TResult> resultHandler)
        {
            if(resultHandler is null)
                throw new ArgumentNullException(nameof(resultHandler));
            _resultHandler = resultHandler;
            return this;
        }

        public Callback<TResult> OnError(Action<string> errorHandler)
        {
            if(errorHandler is null)
                throw new ArgumentNullException(nameof(errorHandler));
            _errorHandler = errorHandler;
            return this;
        }

        public Callback<TResult> OnError(ICallback other) => OnError(err => other.Error(err));
    }

    public sealed class Callback : ICallback
    {
        Action _successHandler;
        Action<string> _errorHandler;
        int _called;

        public void Success()
        {
            if(Interlocked.CompareExchange(ref _called, 1, 0) == 1)
            {
                throw new CallbackAlreadyCalledException();
            }
            _successHandler?.Invoke();
        }

        public void Error(string errorMessage)
        {
            if(Interlocked.CompareExchange(ref _called, 1, 0) == 1)
            {
                throw new CallbackAlreadyCalledException();
            }
            _errorHandler?.Invoke(errorMessage);
        }

        public Callback OnError(ICallback other) => OnError(err => other.Error(err));

        public Callback OnError(Action<string> errorHandler)
        {
            if(errorHandler is null)
                throw new ArgumentNullException(nameof(errorHandler));
            _errorHandler = errorHandler;
            return this;
        }

        public Callback OnSuccess(Action successCallback)
        {
            if(successCallback is null)
                throw new ArgumentNullException(nameof(successCallback));
            _successHandler = successCallback;
            return this;
        }
    }
}
