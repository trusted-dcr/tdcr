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
            System.Console.Write("Starting daemon...");
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

            Config(new ConfigOptions
            {
                RpcPort = opts.RpcPort,
                ConfigPath = opts.ConfigPath
            });

            System.Console.WriteLine($"rpc-port: {opts.RpcPort}");
        }

        public static void Stop(StopOptions opts)
        {
            if (!TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client))
                return;

            try
            {
                client.Stop(new Empty());
            }
            catch {}
        }

        public static void Config(ConfigOptions opts)
        {
            if (!TryReadConfig(opts.ConfigPath, out CoreLib.Messages.Config.SgxConfig config))
                return;

            if (!TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client))
                return;

            try
            {
                client.Config(config.ToWire());
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error while configuring enclave: {ex.Message}");
            }
        }

        public static void Execute(ExecuteOptions opts)
        {
            if (!TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client))
                return;

            try
            {
                client.Execute(new Uid(opts.Event).ToWire());
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error while executing: {ex.Message}");
            }
        }

        public static void CollectGlobalHistory(CollectGlobalHistoryOptions opts)
        {
            CoreLib.Messages.Config.SgxConfig config = null;
            if (!string.IsNullOrEmpty(opts.ConfigPath) && !TryReadConfig(opts.ConfigPath, out config))
                return;

            if (!TryConnectRpc(opts, out SgxDaemon.SgxDaemonClient client))
                return;

            System.Console.WriteLine("Collecting history...");
            Snapshot snapshot;
            try
            {
                snapshot = client.History(new Empty(), new CallOptions(deadline: DateTime.MaxValue));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error while collecting history: {ex.Message}");
                return;
            }

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
            foreach (EventExecution exec in cs.GlobalHistory)
            {
                string eventName = config?.Workflow.Events.First(e => e.Uid == exec.Event).Name;

                System.Console.WriteLine();
                System.Console.ForegroundColor = ConsoleColor.DarkGreen;
                System.Console.WriteLine($"exec  {exec.ExecutionID}");
                System.Console.ResetColor();
                System.Console.WriteLine($"euid  {exec.Event}");
                if (eventName != null)
                    System.Console.WriteLine($"event {eventName}");
            }
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

        private static bool TryReadConfig(string path, out CoreLib.Messages.Config.SgxConfig config)
        {
            try
            {
                string input = File.ReadAllText(path);
                config = JsonConvert.DeserializeObject<CoreLib.Messages.Config.SgxConfig>(input);
                return true;
            }
            catch
            {
                System.Console.WriteLine($"Unable to read configuration at: {path}");
                config = null;
                return false;
            }
        }
    }
}
