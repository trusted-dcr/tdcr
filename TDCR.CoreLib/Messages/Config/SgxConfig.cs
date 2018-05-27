using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TDCR.CoreLib.Messages.Network;
using TDCR.CoreLib.Wire.Dcr;

namespace TDCR.CoreLib.Messages.Config
{
    public class SgxConfig : IPayload<Wire.Sgx.SgxConfig>
    {
        public Uid UID { get; set; }
        public Workflow Workflow { get; set; }
        public Peer[] Peers { get; set; }

        public Wire.Sgx.SgxConfig ToWire()
        {
            var conf = new Wire.Sgx.SgxConfig
            {
                Self = UID.ToWire(),
                Workflow = Workflow
            };

            foreach (var p in Peers)
                conf.Peers.Add(new Wire.Sgx.SgxConfig.Types.Peer { Address = BitConverter.ToUInt32(p.Address.GetAddressBytes().Reverse().ToArray(), 0) });

            return conf;
        }
    }
}
