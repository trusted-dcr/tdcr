using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TDCR.CoreLib.HistoryCollection;
using TDCR.CoreLib.Messages.Config;
using TDCR.CoreLib.Messages.Network;
using TDCR.CoreLib.Messages.Raft;

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
            // Check for first time setup?
            // Read JSON config
            var input = System.IO.File.ReadAllText(opts.ConfigPath);

            var graph = JsonConvert.DeserializeObject<Graph>(input);

            // Start enclave?
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
    }
}
