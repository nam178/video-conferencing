using System.Threading.Tasks;

namespace MediaServer.Mediators
{
    interface ICommandHandler<TArg1, TArg2>
    {
        Task HandleAsync(TArg1 arg1, TArg2 arg2);
    }
}
