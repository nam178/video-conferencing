using System.Threading.Tasks;

namespace MediaServer.Common.Patterns
{
    public interface IMapper<TArg1, TArg2, TResult>
    {
        Task<TResult> HandleAsync(TArg1 arg1, TArg2 arg2);
    }
}
