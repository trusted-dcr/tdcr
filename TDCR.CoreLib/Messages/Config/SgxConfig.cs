using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Tdcr.Sgxd;
using TDCR.CoreLib.Messages.Network;
using TDCR.CoreLib.Wire.Dcr;

namespace TDCR.CoreLib.Messages.Config
{
    public class SgxConfig : IPayload<Tdcr.Sgxd.SgxConfig>
    {
        public Uid UID { get; set; }
        public Workflow Workflow { get; set; }
        public Peer[] Peers { get; set; }

        public Tdcr.Sgxd.SgxConfig ToWire()
        {
            var conf = new Tdcr.Sgxd.SgxConfig
            {
                Self = UID.ToWire(),
                Workflow = Workflow
            };

            foreach (var p in Peers)
                conf.Peers.Add(new Tdcr.Sgxd.SgxConfig.Types.Peer { Address = BitConverter.ToUInt32(p.Address.GetAddressBytes().Reverse().ToArray(), 0) });

            return conf;
        }
    }
}
