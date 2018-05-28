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

    [Verb("start", HelpText = "Starts a daemon process.")]
    public class StartOptions : RpcOptions
    {
        [Option('c', "config", HelpText = "Load daemon process with config located at given path.")]
        public string ConfigPath { get; set; }

        [Option('p', "port", HelpText = "Port used for inter-node communication.", Default = Defaults.Port)]
        public ushort Port { get; set; }
    }

    [Verb("stop", HelpText = "Stop a daemon process.")]
    public class StopOptions : RpcOptions { }

    [Verb("execute", HelpText = "Forcefully execute commands on daemon.")]
    public class ExecuteOptions : RpcOptions
    {

    }

    [Verb("history", HelpText = "Collect the global history of the graph.")]
    public class CollectGlobalHistoryOptions : RpcOptions { }

    [Verb("log", HelpText = "Retrieve the log of the hosted event.")]
    public class RetrieveLogOptions : RpcOptions { }

}
