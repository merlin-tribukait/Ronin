using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Utilities
{
    public class PortChecker
    {
        private static readonly log4net.ILog log = LogHelper.GetLogger();

        private static List<int> usedPorts = new List<int>();
        private static List<int> takenPorts = new List<int>();

        public static int GetOpenPort()
        {
            Random rnd = new Random();
            int portStartIndex = rnd.Next(2000, 3000);
            int portEndIndex = 10000;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

            usedPorts = tcpEndPoints.Select(p => p.Port).ToList<int>();
            int unusedPort = 0;

            for (int port = portStartIndex; port < portEndIndex; port++)
            {
                if (!usedPorts.Contains(port) && !takenPorts.Contains(port))
                {
                    unusedPort = port;
                    break;
                }
            }

            if (unusedPort == 0)
            {
                log.Fatal("Unusable machine ports!");
                Environment.Exit(0);
            }

            takenPorts.Add(unusedPort);
            return unusedPort;
        }
    }
}
