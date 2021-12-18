using System.Threading.Tasks;

namespace TabletBot.Common
{
    public interface IAsyncInitialize
    {
        Task InitializeAsync();
    }
}