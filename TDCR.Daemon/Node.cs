//using Google.Protobuf;
//using Grpc.Core;
//using NLog;
//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using TDCR.CoreLib.Network;
//using TDCR.CoreLib.Messages.Network;
//using TDCR.CoreLib.Wire.Discovery;
//using TDCR.Daemon.Wire;
//using static TDCR.Daemon.Wire.Api;
//using Google.Protobuf.WellKnownTypes;
//using TDCR.CoreLib.Database;
//using TDCR.CoreLib.Routine;

//namespace TDCR.Daemon
//{
//    public class Node : ApiBase
//    {
//        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
//        private readonly Router router;
//        private readonly Server server;

//        public Node(ushort routerPort, ushort rpcPort)
//        {
//            Uid uid = new Uid {
//                Part1 = 123,
//                Part2 = 456
//            };
//            router = new Router(routerPort, uid, LogManager.GetLogger(nameof(Router)));

//            Addr[] peers = PeerStore.Instance.ReadAllPeers();
//            Discovery discovery = new Discovery(peers);
//            discovery.Attach(router, LogManager.GetLogger(nameof(Discovery)));

//            server = new Server
//            {
//                Services = { BindService(this) },
//                Ports = { new ServerPort("localhost", rpcPort, ServerCredentials.Insecure) }
//            };
//            server.Start();
//            logger.Info("gRPC server started at localhost:{port}", rpcPort);
//        }

//        public override Task<SemVer> Version(Empty request, ServerCallContext context)
//        {
//            return Task.FromResult(Constants.Version.SemVer);
//        }

//        public override async Task<Empty> ConnectTo(CoreLib.Wire.Network.Addr addr, Grpc.Core.ServerCallContext context)
//        {
//            IPAddress ip = new IPAddress(addr.Ip.ToByteArray());
//            ushort port = (ushort)addr.Port;
//            logger.Info($"Exec ConnectTo({ip}, {port})");
//            router.Connect(new Addr(new IPEndPoint(ip, port)));
//            return new Empty();
//        }

//        public override Task<Empty> Stop(Empty request, ServerCallContext context)
//        {
//            logger.Info("Got call to stop ({peer})", context.Peer);
//            Environment.Exit(0);
//            return Task.FromResult(new Empty());
//        }

//        public override async Task GetPeers(Empty request, IServerStreamWriter<CoreLib.Wire.Network.Addr> responseStream, ServerCallContext context)
//        {
//            Addr[] peers = PeerStore.Instance.ReadAllPeers();
//            foreach (var peer in peers)
//                await responseStream.WriteAsync(peer.ToWire());
//        }

//        public override Task<Empty> AddPeer(CoreLib.Wire.Network.Addr request, ServerCallContext context)
//        {
//            Addr peer = Addr.FromWire(request);
//            PeerStore.Instance.AppendPeers(new[] { peer });
//            return Task.FromResult(new Empty());
//        }

//        public override Task<Empty> TruncatePeers(Empty request, ServerCallContext context)
//        {
//            PeerStore.Instance.Truncate();
//            return Task.FromResult(new Empty());
//        }
//    }
//}
