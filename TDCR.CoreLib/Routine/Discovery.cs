using NLog;
using System.Collections.Generic;
using TDCR.CoreLib.Network;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Routine
{
    public class Discovery : IRoutine
    {
        public List<Addr> KnownPeers { get; set; }

        private IRouter router;

        public void Attach(IRouter router, ILogger logger = null)
        {
            this.router = router;
        }

        public void GetPeers(Uid target)
        {

        }
    }
}
