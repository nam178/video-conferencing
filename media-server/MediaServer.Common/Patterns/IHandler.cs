using System.Threading.Tasks;

namespace MediaServer.Common.Patterns
{
    public interface IHandler<TArg1, TArg2, TArgs3, TArgs4>
    {
        Task HandleAsync(TArg1 arg1, TArg2 arg2, TArgs3 arg3, TArgs4 arg4);
    }

    public interface IHandler<TArg1, TArg2, TArgs3>
    {
        Task HandleAsync(TArg1 arg1, TArg2 arg2, TArgs3 arg3);
    }

    public interface IHandler<TArg1, TArg2>
    {
        Task HandleAsync(TArg1 arg1, TArg2 arg2);
    }

    public interface IHandler<TArg>
    {
        Task HandleAsync(TArg args);
    }
}
