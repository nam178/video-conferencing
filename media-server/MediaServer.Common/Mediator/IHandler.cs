using System.Threading.Tasks;

namespace MediaServer.Common.Mediator
{
    public interface IHandler<TArg1, TArg2>
    {
        Task HandleAsync(TArg1 arg1, TArg2 arg2);
    }

    public interface IHandler<TArg>
    {
        Task HandleAsync(TArg args);
    }
}
