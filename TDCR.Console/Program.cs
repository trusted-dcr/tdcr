using CommandLine;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            Parser.Default.ParseArguments<StartOptions, StopOptions, ConfigOptions, ExecuteOptions, CollectGlobalHistoryOptions, RetrieveLogOptions>(args)
                .WithParsed((StartOptions opts) => Start(opts))
                .WithParsed((StopOptions opts) => Stop(opts))
                .WithParsed((ConfigOptions opts) => Config(opts))
                .WithParsed((ExecuteOptions opts) => Execute(opts))
                .WithParsed((CollectGlobalHistoryOptions opts) => CollectGlobalHistory(opts))
                .WithParsed((RetrieveLogOptions opts) => RetrieveLog(opts));
        }

        public static void Start(StartOptions opts)
        {
            var input = File.ReadAllText(opts.ConfigPath);
            var config = JsonConvert.DeserializeObject<CoreLib.Messages.Config.SgxConfig>(input);

            System.Console.Write("Starting daemon...");

            System.Console.WriteLine(Path.Combine(AppContext.BaseDirectory, Defaults.DaemonPath));

            Process daemon = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.Combine(AppContext.BaseDirectory, Defaults.DaemonPath),
                    FileName = "sgx_daemon.exe",
                    Arguments = $"{opts.RpcPort}",
                    UseShellExecute = true,
                    CreateNoWindow = opts.Background
                }
            };

            try
            {
                daemon.Start();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"FAILED ({ex.Message})");
                return;
            }
            System.Console.WriteLine("OK");

            // Get daemon version
            if (!TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client))
                return;

            client.Config(config.ToWire());

            System.Console.WriteLine($"rpc-port: {opts.RpcPort}");
        }

        public static void Stop(StopOptions opts)
        {
            if (!TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client))
                return;

            client.Stop(new Empty());
        }

        public static void Config(ConfigOptions opts)
        {
            var input = File.ReadAllText(opts.ConfigPath);
            var config = JsonConvert.DeserializeObject<CoreLib.Messages.Config.SgxConfig>(input);

            if (!TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client))
                return;

            client.Config(config.ToWire());
        }

        public static void Execute(ExecuteOptions opts)
        {
            if (!TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client))
                return;

            client.Execute(new Uid(opts.Event).ToWire());
        }

        public static void CollectGlobalHistory(CollectGlobalHistoryOptions opts)
        {
            if (!TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client))
                return;

            System.Console.WriteLine("Collecting history...");
            Snapshot snapshot = client.History(new Empty(), new CallOptions(deadline: DateTime.MaxValue));

            // Convert snapshot data structure
            var result = new List<Tuple<Uid, Entry[]>>();
            foreach (var part in snapshot.Parts)
            {
                var local = new Tuple<Uid, Entry[]>(
                    Uid.FromWire(part.Cluster),
                    part.Entries.Select(Entry.FromWire).ToArray());

                result.Add(local);
            }

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
