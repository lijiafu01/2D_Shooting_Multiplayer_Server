using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net.Config;
using Photon.SocketServer;
using System.IO;
using System.Threading;

namespace GameServer
{
    public class GameServerApplication : ApplicationBase
    {
        private readonly ILogger Log = LogManager.GetCurrentClassLogger();
        World world = new World();
        //moi khi co nguoi dung moi,se tu dong tao ra lop user
        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return new User(initRequest);
        }

        //init function//vao game se bat dau chay ham khoi tao
        protected override void Setup()
        {
            var file = new FileInfo(Path.Combine(BinaryPath, "log4net.config"));
            if (file.Exists)
            {
                LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                XmlConfigurator.ConfigureAndWatch(file);
            }
            world.Init();
            int workerThreads;
            int completePortThreads;
            ThreadPool.SetMaxThreads(6000, 6000);
            ThreadPool.GetMaxThreads(out workerThreads, out completePortThreads);
            Log.Debug("Server is Ready! " + workerThreads + " " + completePortThreads);
        }

        protected override void TearDown()
        {
            Log.Debug("Server was Stoped!");
        }
    }
}
