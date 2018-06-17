using CommandLine;
using System.Collections.Generic;

namespace TDCR.Console
{
    /// <summary>
    /// Defines options specifying how to connect to an RPC server.
    /// </summary>
    public abstract class RpcOptions
    {
        [Option('p', "rpc-port", HelpText = "Port used for RPC communication.", Default = Defaults.RpcPort)]
        public ushort RpcPort { get; set; }
    }

    [Verb("start", HelpText = "Start and configures a daemon process.")]
    public class StartOptions : RpcOptions
    {
        [Option('c', "config", Required = true, HelpText = "Configure daemon process with config located at given path.")]
        public string ConfigPath { get; set; }

        [Option('b', "background", HelpText = "Start daemon in background.")]
        public bool Background { get; set; }
    }

    [Verb("config", HelpText = "Configures a running daemon process.")]
    public class ConfigOptions : RpcOptions
    {
        [Option('c', "config", Required = true, HelpText = "Configure daemon process with config located at given path.")]
        public string ConfigPath { get; set; }
    }

    [Verb("stop", HelpText = "Stop a daemon process.")]
    public class StopOptions : RpcOptions { }

    [Verb("exec", HelpText = "Execute a DCR event.")]
    public class ExecuteOptions : RpcOptions
    {
        [Value(0, HelpText = "UID of event to be executed.")]
        public string Event { get; set; }
    }

    [Verb("history", HelpText = "Collect the global history of the graph.")]
    public class CollectGlobalHistoryOptions : RpcOptions {
        [Option('c', "config", HelpText = "Use config located at given path to translate event UIDs to human-readable names.")]
        public string ConfigPath { get; set; }
    }

    [Verb("log", HelpText = "Retrieve the log of the hosted event.")]
    public class RetrieveLogOptions : RpcOptions { }

}
