using Google.Protobuf;
using System.Threading.Tasks;
using TDCR.CoreLib.Messages;
using TDCR.CoreLib.Messages.Network;
using TDCR.CoreLib.Messages.Discovery;

namespace TDCR.CoreLib.Network
{
    public delegate void ConnHandler(Uid uid, Addr addr, IRouter router);
    public delegate void RecvHandler<T>(Uid sender, T payload, IRouter router) where T : IPayload<IMessage>;
    public delegate void SentHandler<T>(Uid target, T payload, IRouter router) where T : IPayload<IMessage>;
    public delegate void RecvRawHandler<T>(Addr sender, T payload, IRouter router) where T : IPayload<IMessage>;
    public delegate void SentRawHandler<T>(Addr target, T payload, IRouter router) where T : IPayload<IMessage>;

    public interface IRouter
    {
        event ConnHandler Connected;
        event ConnHandler Disconnected;

        event RecvHandler<Hello> RecvHello;
        event RecvHandler<HelloAck> RecvHelloAck;
        event RecvHandler<GetPeers> RecvGetPeers;
        event RecvHandler<PeerList> RecvPeerList;

        event RecvRawHandler<Hello> RecvRawHello;
        event RecvRawHandler<HelloAck> RecvRawHelloAck;
        event RecvRawHandler<GetPeers> RecvRawGetPeers;
        event RecvRawHandler<PeerList> RecvRawPeerList;

        // SentHello is missing on purpose - target UID would be unknown
        event SentHandler<HelloAck> SentHelloAck;
        event SentHandler<GetPeers> SentGetPeers;
        event SentHandler<PeerList> SentPeerList;

        event SentRawHandler<Hello> SentRawHello;
        event SentRawHandler<HelloAck> SentRawHelloAck;
        event SentRawHandler<GetPeers> SentRawGetPeers;
        event SentRawHandler<PeerList> SentRawPeerList;

        Uid OwnUid { get; }

        void Connect(Addr addr);
        Task ConnectAsync(Addr addr);
        void Disconnect(Uid target);

        void Send(Uid target, IPayload<IMessage> payload);
        Task SendAsync(Uid target, IPayload<IMessage> payload);

        void Accept(Container cont);
    }
}
