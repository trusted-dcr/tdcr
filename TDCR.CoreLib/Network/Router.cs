using Google.Protobuf;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TDCR.CoreLib.Messages;
using TDCR.CoreLib.Messages.Network;
using TDCR.CoreLib.Messages.Discovery;

namespace TDCR.CoreLib.Network
{
    public class Router : IRouter, IDisposable
    {
        public event ConnHandler Connected;
        public event ConnHandler Disconnected;

        public event RecvHandler<Hello> RecvHello;
        public event RecvHandler<HelloAck> RecvHelloAck;

        public event RecvRawHandler<Hello> RecvRawHello;
        public event RecvRawHandler<HelloAck> RecvRawHelloAck;

        public event SentHandler<HelloAck> SentHelloAck;

        public event SentRawHandler<Hello> SentRawHello;
        public event SentRawHandler<HelloAck> SentRawHelloAck;

        public Uid OwnUid { get; }

        private ushort port;
        private Dictionary<Uid, TcpClient> conns;

        private TcpListener listener;
        private CancellationTokenSource listenerCancellation;

        private ILogger logger;

        public Router(ushort port, Uid ownUid, ILogger logger = null)
        {
            this.port = port;
            OwnUid = ownUid;
            this.logger = logger;
            conns = new Dictionary<Uid, TcpClient>();

            if (logger != null)
            {
                Connected += (uid, addr, _) => logger?.Info($"Connected -- {addr}/{uid}");
                Disconnected += (uid, addr, _) => logger?.Info($"Disconnected -- {addr}/{uid}");

                RecvRawHello += (addr, p, r) => logger?.Debug($"HELLO <- {addr}");
                RecvRawHelloAck += (addr, p, r) => logger?.Debug($"HELLOACK <- {addr}");

                SentRawHello += (addr, p, r) => logger?.Debug($"HELLO -> {addr}");
                SentRawHelloAck += (addr, p, r) => logger?.Debug($"HELLOACK -> {addr}");
            }

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            listenerCancellation = new CancellationTokenSource();
            listenerCancellation.Token.Register(listener.Stop);
            new Task(async () => {
                while (true)
                {
                    TcpClient conn = await listener.AcceptTcpClientAsync();
                    logger?.Debug($"TCP <- {GetAddr(conn)}");
                    logger?.Info($"Starting handshake <- {GetAddr(conn)}");
                    HandshakeAsync(conn);
                }
            }, listenerCancellation.Token, TaskCreationOptions.LongRunning).Start();
        }

        public void Connect(Addr addr)
        {
            ConnectAsync(addr).Wait();
        }

        public async Task ConnectAsync(Addr addr)
        {
            TcpClient conn = new TcpClient();
            await conn.ConnectAsync(addr.IP, addr.Port);
            logger?.Debug($"TCP -> {addr}");
            logger?.Info($"Starting handshake -> {conn}");
            HandshakeAsync(conn);
        }

        public void Disconnect(Uid target)
        {
            if (!conns.ContainsKey(target))
                throw new ArgumentException($"Not connected to {target}", nameof(target));
            conns.Remove(target);
            Disconnect(conns[target]);
        }

        private void Disconnect(TcpClient conn)
        {
            logger?.Debug($"TCP -/-> {GetAddr(conn)}");
            conn.Close();
        }

        public void Send(Uid target, IPayload<IMessage> payload)
        {
            SendAsync(target, payload).Wait();
        }

        public async Task SendAsync(Uid target, IPayload<IMessage> payload)
        {
            if (!conns.ContainsKey(target))
                throw new ArgumentException($"Not connected to {target}", nameof(target));
            await SendAsync(conns[target], payload);

            // Raise sent-event
            switch (payload)
            {
                case HelloAck p:
                    SentHelloAck(target, p, this);
                    break;
                default:
                    throw new ArgumentException("Unknown payload type", nameof(payload));
            }
        }

        private async Task SendAsync(TcpClient conn, IPayload<IMessage> payload)
        {
            Container cont = new Container
            {
                Sender = OwnUid,
                Payload = payload
            };

            MemoryStream buffer = new MemoryStream();
            cont.ToWire().WriteDelimitedTo(buffer);
            buffer.Position = 0;
            await buffer.CopyToAsync(conn.GetStream());

            // Raise raw sent-event
            Addr addr = GetAddr(conn);
            switch (payload)
            {
                case Hello p:
                    SentRawHello?.Invoke(addr, p, this);
                    break;
                case HelloAck p:
                    SentRawHelloAck?.Invoke(addr, p, this);
                    break;
                default:
                    throw new ArgumentException("Unknown payload type", nameof(payload));
            }
        }

        public void Accept(Container cont)
        {
            switch (cont.Payload)
            {
                case Hello p:
                    RecvHello?.Invoke(cont.Sender, p, this);
                    break;
                case HelloAck p:
                    RecvHelloAck?.Invoke(cont.Sender, p, this);
                    break;
                default:
                    throw new ArgumentException("Unknown payload type", nameof(cont.Payload));
            }
        }

        private Container Receive(TcpClient conn)
        {
            Wire.Network.Container wire = Wire.Network.Container.Parser.ParseDelimitedFrom(conn.GetStream());
            Container cont = Container.FromWire(wire);

            Addr addr = GetAddr(conn);
            switch (cont.Payload)
            {
                case Hello p:
                    RecvRawHello?.Invoke(addr, p, this);
                    break;
                case HelloAck p:
                    RecvRawHelloAck?.Invoke(addr, p, this);
                    break;
                default:
                    throw new ArgumentException("Unknown payload type", nameof(cont.Payload));
            }
            return cont;
        }

        public void Dispose()
        {
            listenerCancellation.Cancel();
            throw new NotImplementedException();
        }

        [Flags]
        private enum PeerStatus
        {
            Waiting = 0,
            RecvAck = 1 << 1,
            SentAck = 1 << 2,
            Connected = RecvAck | SentAck
        }

        private async Task HandshakeAsync(TcpClient conn)
        {
            Addr addr = GetAddr(conn);

            // Send HELLO
            SendAsync(conn, new Hello
            {
                Receiver = addr,
                Version = Constants.Version
            });

            // Recv HELLOACK and HELLO
            PeerStatus status = PeerStatus.Waiting;
            Uid peer = null;
            while (status != PeerStatus.Connected)
            {
                // Receive single message
                Container cont = Receive(conn);

                if (cont.Payload is Hello hello && !status.HasFlag(PeerStatus.SentAck))
                {
                    // Version compatibility check
                    if (hello.Version != Constants.Version)
                    {
                        logger?.Warn($"Handshake failed ({addr}): Incompatible verison");
                        break;
                    }
                    peer = cont.Sender;

                    // Send HELLOACK
                    // Await so we get the TCP ACK before updating the status
                    await SendAsync(conn, new HelloAck());
                    status |= PeerStatus.SentAck;
                }
                else if (cont.Payload is HelloAck ack && !status.HasFlag(PeerStatus.RecvAck))
                {
                    status |= PeerStatus.RecvAck;
                }
                else
                {
                    logger?.Warn($"Handshake failed ({addr}): Invalid message ({cont.Payload.GetType()})");
                    break;
                }
            }

            // If handshake was failure, dispose of connection
            if (status != PeerStatus.Connected)
            {
                conn.Close();
                return;
            }

            // Register UID to connection
            conns[peer] = conn;
            Connected?.Invoke(peer, addr, this);

            // Start general message receive loop
            new Task(() =>
            {
                while (conn.Connected)
                    Receive(conn);
                Disconnected?.Invoke(peer, addr, this);
            }, TaskCreationOptions.LongRunning).Start();
        }

        public static Addr GetAddr(TcpClient conn)
        {
            return new Addr((IPEndPoint)conn.Client.RemoteEndPoint);
        }
    }
}
