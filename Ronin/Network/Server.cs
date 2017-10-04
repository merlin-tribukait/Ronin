using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Utilities;

namespace Ronin.Network
{
    /// <summary>
    ///     TCP Proxy Server Implementation
    /// </summary>
    public class Server
    {
        /// <summary>
        /// List used to stop all running servers on application exit.
        /// </summary>
        public static List<Server> AllServers = new List<Server>();

        /// <summary>
        ///     Local copy of our connected client.
        /// </summary>
        private Client _client;

        /// <summary>
        /// Collection with all the connections.
        /// </summary>
        private Dictionary<int, Client> _clients = new Dictionary<int, Client>();

        /// <summary>
        /// List with all the active proxy tcps for the gameservers.
        /// </summary>
        public static List<Server> GameServers = new List<Server>();

        /// <summary>
        ///     Local listening server object.
        /// </summary>
        private TcpListener _server;

        /// <summary>
        /// Reference to the instance of the injector that is responsible for the relevant game client.
        /// </summary>
        public Injector L2Injector;

        /// <summary>
        /// Determining whether the tcp server is a proxy to authentification or gameserver.
        /// </summary>
        public bool IsAuthServer;

        public L2Bot BotInstance;

        /// <summary>
        ///     Default Constructor
        /// </summary>
        public Server()
        {
            // Setup class defaults..
            LocalAddress = IPAddress.Loopback.ToString();
            LocalPort = 7776;
            RemoteAddress = IPAddress.Loopback.ToString();
            RemotePort = 7777;
        }

        /// <summary>
        ///     Gets or sets the local address of this listen server.
        /// </summary>
        public string LocalAddress { get; set; }

        /// <summary>
        ///     Gets or sets the local port of this listen server.
        /// </summary>
        public int LocalPort { get; set; }

        /// <summary>
        ///     Gets or sets the remote address to forward the client to.
        /// </summary>
        public string RemoteAddress { get; set; }

        /// <summary>
        ///     Gets or sets the remote port to foward the client to.
        /// </summary>
        public int RemotePort { get; set; }

        /// <summary>
        ///     Gets or sets the server id. (Naia - 16, Chronos - 15 ...
        /// </summary>
        public int ServerId { get; set; }

        /// <summary>
        ///     Local copy of our connected client.
        /// </summary>
        public Client Client
        {
            get { return _client; }
            set { _client = value; }
        }

        /// <summary>
        ///     Starts our listen server to accept incoming connections.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            try
            {
                // Cleanup any previous objects..
                Stop();

                // Create the new TcpListener..
                _server = new TcpListener(IPAddress.Parse(LocalAddress), LocalPort);
                _server.Start();

                // Setup the async handler when a client connects..
                _server.BeginAcceptTcpClient(OnAcceptTcpClient, _server);

                AllServers.Add(this);
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        ///     Stops the local listening server if it is started.
        /// </summary>
        public void Stop()
        {
            // Cleanup the client object..
            //_client?.Stop();
            //_client = null;

            // Cleanup the server object..
            _server?.Stop();
            _server = null;
        }

        /// <summary>
        ///     Async callback handler that accepts incoming TcpClient connections.
        ///     NOTE:
        ///     It is important that you use the results server object to
        ///     prevent threading issues and object disposed errors!
        /// </summary>
        /// <param name="result"></param>
        private void OnAcceptTcpClient(IAsyncResult result)
        {
            //LogHelper.GetLogger().Debug(IsAuthServer);
            //LogHelper.GetLogger().Debug("OnAcceptTcpClient");
            // Ensure this connection is complete and valid..
            try
            {
                if (result.IsCompleted == false || !(result.AsyncState is TcpListener))
                {
                    Stop();
                    return;
                }
            }
            catch (Exception e)
            {
                LogHelper.GetLogger().Debug(e);
                Stop();
                return;
            }

            // Obtain our server instance. (YOU NEED TO USE IT LIKE THIS DO NOT USE this._server here!)
            var tcpServer = result.AsyncState as TcpListener;
            try
            {
                // End the async connection request..
                TcpClient tcpClient;
                try
                {
                    tcpClient = tcpServer.EndAcceptTcpClient(result);
                }
                catch
                {
                    return;
                }

                // Prepare the client and start the proxying..
                _client = new Client(tcpClient.Client);
                _client.isAuth = this.IsAuthServer;
                _client.Start(RemoteAddress, RemotePort);
                _client.ServerId = ServerId;
                _client.L2Injector = L2Injector;
                _client.BotInstance = this.BotInstance;
                BotInstance.PlayerData.GameState = IsAuthServer ? GameState.AccountLogin : GameState.CharacterSelection;
                //if (!IsAuthServer)
                //{
                //    L2Bot bot = new L2Bot(_client);
                //    L2Bot.Bots.Add(bot);
                //}
                //Add them to a dictionary, so we can monitor the connections
                //int port = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port;
                //_clients.Add(port, _client);
            }
            catch
            {
                Stop();

                //Debug.WriteLine("Error while attempting to complete async connection.");
            }

            // Begin listening for the next client..
            tcpServer.BeginAcceptTcpClient(OnAcceptTcpClient, tcpServer);
        }
    }
}
