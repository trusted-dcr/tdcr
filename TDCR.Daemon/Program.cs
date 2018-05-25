using NLog;
using System;

namespace TDCR.Daemon
{
    public class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            if (args.Length != 2 || !ushort.TryParse(args[0], out ushort port) || !ushort.TryParse(args[1], out ushort rpcPort))
            {
                logger.Error("Invalid arguments");
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((_, eargs) => {
                logger.Error((Exception)eargs.ExceptionObject);
            });

            logger.Error($"Attaching deamon to port {port} (rpc {rpcPort})");
            //Node node = new Node(port, rpcPort);

            //LogFactory lf = new LogFactory();
            //Logger rl = lf.GetLogger(nameof(Router));


            //UID uid = new UID { Data = ByteString.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }) };
            //Router r = new Router(uid, port, rl);
            //r.Start();

            //Router r2 = new Router(uid, 8001, rl);
            //r2.Start();
            //r2.Connect(new IPEndPoint(IPAddress.Loopback, 8000)).Wait();

            Console.ReadLine();

            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
        }
    }
}
