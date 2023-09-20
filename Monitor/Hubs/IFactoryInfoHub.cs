using Shared;

namespace Monitor.Hubs
{
    public interface IFactoryInfoHub
    {
        Task SendInfo(FactoryInfo info);
    }
}
