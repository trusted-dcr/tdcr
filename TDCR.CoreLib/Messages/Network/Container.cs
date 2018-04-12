﻿using Google.Protobuf;
using System;
using TDCR.CoreLib.Messages.Discovery;

namespace TDCR.CoreLib.Messages.Network
{
    public class Container: IPayload<Wire.Network.Container>
    {
        public Uid Sender { get; set; }
        public IPayload<IMessage> Payload { get; set; }

        public Wire.Network.Container ToWire()
        {
            Wire.Network.Container.Types.PayloadType type;
            switch (Payload)
            {
                case Hello _:
                    type = Wire.Network.Container.Types.PayloadType.HelloMsg;
                    break;
                case HelloAck _:
                    type = Wire.Network.Container.Types.PayloadType.HelloAck;
                    break;
                default:
                    throw new InvalidOperationException("Unknown payload type");
            }

            return new Wire.Network.Container
            {
                Sender = Sender.ToWire(),
                Type = type,
                Payload = ByteString.CopyFrom(Payload.ToWire().ToByteArray())
            };
        }

        public static Container FromWire(Wire.Network.Container wire)
        {
            IPayload<IMessage> payload;
            switch (wire.Type)
            {
                case Wire.Network.Container.Types.PayloadType.HelloMsg:
                    payload = Hello.FromWire(Wire.Discovery.HelloMsg.Parser.ParseFrom(wire.Payload));
                    break;
                case Wire.Network.Container.Types.PayloadType.HelloAck:
                    payload = HelloAck.FromWire(Wire.Discovery.HelloAck.Parser.ParseFrom(wire.Payload));
                    break;
                default:
                    throw new ArgumentException("Unknown wire payload type", nameof(wire.Type));
            }

            return new Container
            {
                Sender = Uid.FromWire(wire.Sender),
                Payload = payload
            };
        }
    }
}