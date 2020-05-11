using System;

namespace MediaServer.Common.Patterns
{

    public interface IObserver
    {
        void Error(string errorMessage);
    }

    public sealed class Observer<TResult> : IObserver
    {
        public void Result(TResult result) => _resultHandler?.Invoke(result);

        Action<TResult> _resultHandler;

        public Observer<TResult> OnResult(Action<TResult> resultHandler)
        {
            if(resultHandler is null)
                throw new ArgumentNullException(nameof(resultHandler));
            _resultHandler = resultHandler;
            return this;
        }

        Action<string> _errorHandler;

        public Observer<TResult> OnError(Action<string> errorHandler)
        {
            if(errorHandler is null)
                throw new ArgumentNullException(nameof(errorHandler));
            _errorHandler = errorHandler;
            return this;
        }

        public Observer<TResult> OnError(IObserver other) => OnError(err => other.Error(err));

        public void Error(string errorMessage) => _errorHandler?.Invoke(errorMessage);
    }

    public sealed class Observer : IObserver
    {
        public void Success() => _successCallback?.Invoke();

        Action _successCallback;

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
            _successCallback = successCallback;
            return this;
        }

        Action<string> _errorHandler;

        public void Error(string errorMessage) => _errorHandler?.Invoke(errorMessage);
    }
}
