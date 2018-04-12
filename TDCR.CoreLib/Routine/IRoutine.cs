using NLog;
using TDCR.CoreLib.Network;

namespace TDCR.CoreLib.Routine
{
    public interface IRoutine
    {
        void Attach(IRouter router, ILogger logger = null);
    }
}
