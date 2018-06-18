using Google.Protobuf;
using System;
using System.Collections.Generic;
using TDCR.CoreLib.Messages.Raft;

namespace TDCR.CoreLib.Messages.Network
{
    public class Container : IPayload<Wire.Network.Container>
    {
        private static Dictionary<Type, Wire.Network.Container.Types.PayloadType> types;
        private static Dictionary<Wire.Network.Container.Types.PayloadType, Func<ByteString, IPayload<IMessage>>> funcs;

        public Uid Sender { get; set; }
        public IPayload<IMessage> Payload { get; set; }

        static Container()
        {
            types = new Dictionary<Type, Wire.Network.Container.Types.PayloadType>
            {
                { typeof(AppendRequest), Wire.Network.Container.Types.PayloadType.AppendRequest }
            };

            funcs = new Dictionary<Wire.Network.Container.Types.PayloadType, Func<ByteString, IPayload<IMessage>>>
            {

            };
        }

        public Wire.Network.Container ToWire()
        {
            return new Wire.Network.Container
            {
                Source = Sender.ToWire(),
                Type = types[Payload.GetType()],
                Payload = ByteString.CopyFrom(Payload.ToWire().ToByteArray())
            };
        }

        public static Container FromWire(Wire.Network.Container wire)
        {
            return new Container
            {
                Sender = Uid.FromWire(wire.Source),
                Payload = funcs[wire.Type](wire.Payload)
            };
        }
    }
}
