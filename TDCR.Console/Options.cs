using CommandLine;
using System.Collections.Generic;

namespace TDCR.Console
{
    /// <summary>
    /// Defines options specifying how to connect to an RPC server.
    /// </summary>
    public abstract class RpcOptions
    {
        [Option("rpc-port", HelpText = "Port used for RPC communication.", Default = Defaults.RpcPort)]
        public ushort RpcPort { get; set; }
    }

    [Verb("start", HelpText = "Start a daemon process.")]
    public class StartOptions : RpcOptions
    {
        [Option('p', "port", HelpText = "Port used for inter-node communication.", Default = Defaults.Port)]
        public ushort Port { get; set; }
    }

    [Verb("stop", HelpText = "Stop a daemon process.")]
    public class StopOptions : RpcOptions
    {
    }

    [Verb("peers", HelpText = "List peers in the peer database.")]
    public class PeersOptions : RpcOptions
    {
        [Option('a', "add-peer", HelpText = "Add peer to peer database.")]
        public string AddPeer { get; set; }

        [Option("truncate", HelpText = "Truncate the peer database.")]
        public bool Truncate { get; set; }
    }

    [Verb("exec", HelpText = "Forcefully execute commands on daemon.")]
    public class ExecOptions : RpcOptions
    {
        public enum Command { ConnectTo }

        [Value(0, MetaName = "cmd", HelpText = "Name of the command to be executed.")]
        public Command Cmd { get; set; }

        [Value(1, MetaName = "args", HelpText = "Command arguments.")]
        public IEnumerable<string> Args { get; set; }
    }
}
