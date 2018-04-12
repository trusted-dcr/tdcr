using CommandLine;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using TDCR.Daemon;
using TDCR.Daemon.Wire;
using static TDCR.Daemon.Wire.Api;

namespace TDCR.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<StartOptions, StopOptions, ExecOptions>(args)
                .WithParsed((StartOptions opts) => Start(opts))
                .WithParsed((StopOptions opts) => Stop(opts))
                .WithParsed((ExecOptions opts) => Exec(opts));
        }

        public static void Start(StartOptions opts)
        {
            System.Console.Write("Starting daemon...");
            Process daemon = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.Combine(AppContext.BaseDirectory, Defaults.DaemonPath),
                    FileName = "dotnet",
                    Arguments = $"TDCR.Daemon.dll {opts.Port} {opts.RpcPort}",
                    UseShellExecute = false,
                    CreateNoWindow = true
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
            if (!TryConnectRpc(opts, out ApiClient client))
                return;
            SemVer version = client.Version(new Empty());

            System.Console.WriteLine($"port:     {opts.Port}");
            System.Console.WriteLine($"rpc-port: {opts.RpcPort}");
            System.Console.WriteLine($"version:  {version.GetVer()}");
        }

        public static void Stop(StopOptions opts)
        {
            if (!TryConnectRpc(opts, out ApiClient client))
                return;

            System.Console.Write($"Stopping daemon...");
            try
            {
                client.Stop(new Empty());
                System.Console.WriteLine("OK");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"FAILED ({ex.Message})");
            }
        }

        public static void Exec(ExecOptions opts)
        {
            if (!TryConnectRpc(opts, out ApiClient client))
                return;

            string[] args = opts.Args.ToArray();
            switch (opts.Cmd)
            {
                case ExecOptions.Command.ConnectTo:
                    if (args.Length != 2 || !IPAddress.TryParse(args[0], out IPAddress ip) || !ushort.TryParse(args[1], out ushort port))
                    {
                        System.Console.WriteLine("Invalid command arguments");
                        return;
                    }

                    client.ConnectTo(new CoreLib.Messages.Network.Addr { IP = ip, Port = port }.ToWire());
                    break;
            }
        }

        private static bool TryConnectRpc(RpcOptions opts, out ApiClient client)
        {
            System.Console.Write($"Connecting to daemon (localhost:{opts.RpcPort})...");
            try
            {
                client = new ApiClient(new Grpc.Core.Channel("localhost", opts.RpcPort, Grpc.Core.ChannelCredentials.Insecure));
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
