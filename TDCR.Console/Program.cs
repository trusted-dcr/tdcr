using CommandLine;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TDCR.CoreLib.HistoryCollection;
using TDCR.CoreLib.Messages.Config;
using TDCR.CoreLib.Messages.Network;
using TDCR.CoreLib.Messages.Raft;
using TDCR.CoreLib.Wire.Sgx;

namespace TDCR.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<StartOptions, StopOptions, ExecuteOptions>(args)
                .WithParsed((StartOptions opts) => Start(opts))
                .WithParsed((StopOptions opts) => Stop(opts))
                .WithParsed((ExecuteOptions opts) => Execute(opts))
                .WithParsed((CollectGlobalHistoryOptions opts) => CollectGlobalHistory(opts))
                .WithParsed((RetrieveLogOptions opts) => RetrieveLog(opts));
        }

        public static void Start(StartOptions opts)
        {
            var input = File.ReadAllText(opts.ConfigPath);
            var config = JsonConvert.DeserializeObject<CoreLib.Messages.Config.SgxConfig>(input);

            System.Console.Write("Starting daemon...");

            //Process daemon = new Process
            //{
            //    StartInfo = new ProcessStartInfo
            //    {
            //        WorkingDirectory = Path.Combine(AppContext.BaseDirectory, Defaults.DaemonPath),
            //        FileName = "dotnet",
            //        Arguments = $"TDCR.Daemon.dll {opts.Port} {opts.RpcPort}",
            //        UseShellExecute = false,
            //        CreateNoWindow = true
            //    }
            //};

            //try
            //{
            //    daemon.Start();
            //}
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine($"FAILED ({ex.Message})");
            //    return;
            //}
            //System.Console.WriteLine("OK");

            // Get daemon version
            if (!TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client))
                return;

            client.Config(config.ToWire());

            System.Console.WriteLine($"rpc-port: {opts.RpcPort}");
        }

        public static void Stop(StopOptions opts)
        {
            // Stop enclave
        }

        public static void Execute(ExecuteOptions opts)
        {
            // TODO: Enclave call with auth.
        }

        public static void CollectGlobalHistory(CollectGlobalHistoryOptions opts)
        {
            System.Console.WriteLine("Collecting history...");

            TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client);

            // TODO: Enclave call
            var result = new List<Tuple<Uid, Entry[]>>();

            var cs = new CheapShot(result);

            var historySB = new StringBuilder("Global history: (");
            for (int i = 0; i < cs.GlobalHistory.Length; i++)
            {
                // TODO: Look-up human names
                var name = cs.GlobalHistory[i].Event + "_" + cs.GlobalHistory[i].ExecutionID;

                historySB.Append(name);
                if (i + 1 < cs.GlobalHistory.Length)
                    historySB.Append(", ");
            }
            historySB.Append(")");

            System.Console.WriteLine(historySB.ToString());
        }

        public static void RetrieveLog(RetrieveLogOptions opts)
        {
            System.Console.WriteLine("Retrieving log...");

            // TODO: Enclave call
            var log = new string[] { "" };
            foreach (var e in log)
            {
                System.Console.WriteLine(e);
            }

            System.Console.WriteLine("End of log");
        }

        private static bool TryConnectRpc(RpcOptions opts, out CoreLib.Wire.Sgx.SgxDaemon.SgxDaemonClient client)
        {
            System.Console.Write($"Connecting to daemon (localhost:{opts.RpcPort})...");
            try
            {
                client = new CoreLib.Wire.Sgx.SgxDaemon.SgxDaemonClient(new Grpc.Core.Channel("localhost", opts.RpcPort, Grpc.Core.ChannelCredentials.Insecure));
                System.Console.WriteLine("OK");
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"FAILED ({ex.Message})");
                client = null;
                return false;
            }
        }
    }
}
