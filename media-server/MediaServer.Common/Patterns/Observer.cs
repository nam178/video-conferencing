using System;
using System.Threading;

namespace MediaServer.Common.Patterns
{
    public interface IObserver
    {
        void Error(string errorMessage);
    }

    public sealed class ObserverAlreadyCompletedException : System.Exception { }

    public sealed class Observer<TResult> : IObserver
    {
        Action<TResult> _resultHandler;
        Action<string> _errorHandler;
        int _called;

        public void Result(TResult result)
        {
            if(Interlocked.CompareExchange(ref _called, 1, 0) == 1)
            {
                throw new ObserverAlreadyCompletedException();
            }
            _resultHandler?.Invoke(result);
        }

        public void Error(string errorMessage)
        {
            if(Interlocked.CompareExchange(ref _called, 1, 0) == 1)
            {
                throw new ObserverAlreadyCompletedException();
            }
            _errorHandler?.Invoke(errorMessage);
        }

        public Observer<TResult> OnResult(Action<TResult> resultHandler)
        {
            if(resultHandler is null)
                throw new ArgumentNullException(nameof(resultHandler));
            _resultHandler = resultHandler;
            return this;
        }

        public Observer<TResult> OnError(Action<string> errorHandler)
        {
            if(errorHandler is null)
                throw new ArgumentNullException(nameof(errorHandler));
            _errorHandler = errorHandler;
            return this;
        }

        public Observer<TResult> OnError(IObserver other) => OnError(err => other.Error(err));
    }

    public sealed class Observer : IObserver
    {
        Action _successHandler;
        Action<string> _errorHandler;
        int _called;

        public void Success()
        {
            if(Interlocked.CompareExchange(ref _called, 1, 0) == 1)
            {
                throw new ObserverAlreadyCompletedException();
            }
            _successHandler?.Invoke();
        }

        public void Error(string errorMessage)
        {
            if(Interlocked.CompareExchange(ref _called, 1, 0) == 1)
            {
                throw new ObserverAlreadyCompletedException();
            }
            _errorHandler?.Invoke(errorMessage);
        }

        public Observer OnError(IObserver other) => OnError(err => other.Error(err));

        public Observer OnError(Action<string> errorHandler)
        {
            if(errorHandler is null)
                throw new ArgumentNullException(nameof(errorHandler));
            _errorHandler = errorHandler;
            return this;
        }

        public Observer OnSuccess(Action successCallback)
        {
            if(successCallback is null)
                throw new ArgumentNullException(nameof(successCallback));
            _successHandler = successCallback;
            return this;
        }
    }
}
