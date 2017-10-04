using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net.Repository.Hierarchy;
using Ronin.Data;
using Ronin.Data.Constants;
using Ronin.Data.Structures;
using Ronin.Network.Cryptography;
using Ronin.Network.Cryptography.Rpg_club;
using Ronin.Network.Cryptography.SmartGuard;
using Ronin.Protocols;
using Ronin.Utilities;

namespace Ronin.Network
{
    public class Client
    {
        public static bool JUSTASNIFFER = false;

        /// <summary>
        /// List used to stop all running servers on application exit.
        /// </summary>
        public static List<Client> AllClients = new List<Client>();

        /// <summary>
        /// Mmaximum amount of data to receive in a single packet.
        /// </summary>
        private static Int32 MAX_BUFFER_SIZE = 65535;

        public int ServerId;

        /// <summary>
        /// Reference to the instance of the injector that is responsible for the relevant game client.
        /// </summary>
        public Injector L2Injector;

        /// <summary>
        /// A flag showing if the first initialization packet is received.
        /// </summary>
        private bool _isInit = false;

        /// <summary>
        /// Internal client state to prevent multiple stop calls.
        /// (Helps reduce the number of unneeded exceptions.)
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Determining whether the tcp client is connected to authentification or gameserver.
        /// </summary>
        public bool isAuth;

        /// <summary>
        /// Client variables.
        /// </summary>
        private Socket _clientSocket;

        /// <summary>
        /// Temporary buffer containing the current stream of bytes received from the client. (Do not confuse this property for Backlog, this once contains only parts of the communication)
        /// </summary>
        private byte[] _clientBuffer;

        /// <summary>
        /// The holistic stream buffer, collection of the byte pieces that the program has received from the client.
        /// </summary>
        private List<byte> _clientBacklog;

        /// <summary>
        /// Server variables.
        /// </summary>
        private Socket _serverSocket;

        /// <summary>
        /// Temporary buffer containing the current stream of bytes received from the server. (Do not confuse this property for Backlog, this once contains only parts of the communication)
        /// </summary>
        private byte[] _serverBuffer;

        /// <summary>
        /// The holistic stream buffer, collection of the byte pieces that the program has received from the server.
        /// </summary>
        private List<byte> _serverBacklog;

        /// <summary>
        /// Game packet factory, initialised for the relative chronicle.
        /// </summary>
        private PacketFactory packetFactory;

        /// <summary>
        /// The Proxy client needs data ref to pass on the packet parser.
        /// </summary>
        //public L2PlayerData data;

        private static readonly log4net.ILog log = LogHelper.GetLogger();

        private Blowfish _blowfishDecypher = new Blowfish();

        internal InGameObfuscator _gamePacketObfuscator;

        internal InGameObfuscator _secondaryGamePacketObfuscator;

        private static object syncLock = new object();

        private static object syncLock2 = new object();

        public L2Bot BotInstance;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="sockClient"></param>
        public Client(Socket sockClient)
        {
            // Setup class defaults..
            this._clientSocket = sockClient;
            this._clientBuffer = new byte[MAX_BUFFER_SIZE];
            this._clientBacklog = new List<byte>();

            this._serverSocket = null;
            this._serverBuffer = new byte[MAX_BUFFER_SIZE];
            this._serverBacklog = new List<byte>();

            AllClients.Add(this);
        }

        public void SendPacketToServer(byte[] packet)
        {
            lock (syncLock)
            {
                _secondaryGamePacketObfuscator?.ObfuscatePacketForServer(packet);
                _gamePacketObfuscator.ObfuscatePacketForServer(packet);
                this.SendToServerRaw(packet);
            }
        }

        public void SendPacketToClient(byte[] packet)
        {
            lock (syncLock)
            {
                _secondaryGamePacketObfuscator?.ObfuscatePacketForServer(packet);
                _gamePacketObfuscator.ObfuscatePacketForClient(packet);
                this.SendToClientRaw(packet);
            }
        }

        /// <summary>
        /// Starts our proxy client.
        /// </summary>
        /// <param name="remoteTarget"></param>
        /// <param name="remotePort"></param>
        /// <returns></returns>
        public bool Start(string remoteTarget = "127.0.0.1", int remotePort = 7777)
        {
            // Stop this client if it was already started before..
            if (this._isRunning == true)
                this.Stop();
            this._isRunning = true;

            // Attempt to parse the given remote target.
            // This allows an IP address or domain to be given.
            // Ex:
            //      127.0.0.1
            //      google.com

            IPAddress ipAddress = null;
            try
            {
                ipAddress = IPAddress.Parse(remoteTarget);
            }
            catch
            {
                try
                {
                    ipAddress = Dns.GetHostEntry(remoteTarget).AddressList[0];
                }
                catch
                {
                    throw new SocketException((int) SocketError.HostNotFound);
                }
            }

            try
            {
                // Connect to the target machine on a new socket..
                this._serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._serverSocket.BeginConnect(new IPEndPoint(ipAddress, remotePort),
                    new AsyncCallback((result) =>
                    {
                        // Ensure the connection was valid..
                        if (result == null || result.IsCompleted == false || !(result.AsyncState is Socket))
                            return;

                        // Obtain our server instance. 
                        Socket serverSocket = ((Socket) result.AsyncState);

                        // Stop processing if the server has told us to stop..
                        if (this._isRunning == false || serverSocket == null)
                            return;

                        // Complete the async connection request..

                        try
                        {
                            serverSocket.EndConnect(result);
                        }
                        catch (Exception e)
                        {
                            //log.Debug(e);
                        }

                        // Start monitoring for packets..
                        this._clientSocket.ReceiveBufferSize = MAX_BUFFER_SIZE;
                        serverSocket.ReceiveBufferSize = MAX_BUFFER_SIZE;
                        this.Server_BeginReceive();
                        this.Client_BeginReceive();
                    }), this._serverSocket);
                return true;
            }
            catch (ObjectDisposedException ex)
            {
                log.Debug(ex.ToString());
            }
            catch (SocketException ex)
            {
                log.Debug(ex.ToString());
            }
            catch (Exception ex)
            {
                log.Debug(ex.ToString());
            }

            return false;
        }

        /// <summary>
        /// Stops this client object.
        /// </summary>
        public void Stop()
        {
            if (this._isRunning == false)
                return;

            if (isAuth)
            {
                _isInit = false;
                int timeout = 0;
                while (
                    Server.GameServers.Where(srvr => Object.ReferenceEquals(srvr.BotInstance, BotInstance))
                        .All(srvr => srvr.Client == null) && timeout < 20)
                {
                    Thread.Sleep(100);
                    timeout++;
                }

                if (
                    Server.GameServers.All(
                        srvr => !Object.ReferenceEquals(srvr.BotInstance, BotInstance) || srvr.Client == null))
                {
                    L2Injector.ReHookClient();
                    BotInstance.PlayerData.GameState = GameState.AccountLogin;
                }
            }
            else
            {
                BotInstance.PlayerData.MainHero = new MainHero();
                BotInstance.PlayerData.Players.Clear();
                BotInstance.PlayerData.Npcs.Clear();
                BotInstance.PlayerData.GameState = GameState.AccountLogin;
                log.Debug("Game client has been disconnected.");
                foreach (var gameServer in Server.GameServers)
                {
                    gameServer.Stop();
                }

                Server.GameServers.Clear();
                L2Injector.ReHookClient();
            }

            // Cleanup the client socket..
            this._clientSocket?.Close();
            this._clientSocket = null;

            // Cleanup the server socket..
            this._serverSocket?.Close();
            this._serverSocket = null;

            this._isRunning = false;
        }

        /// <summary>
        /// Begins an async event to receive incoming data.
        /// </summary>
        private void Client_BeginReceive()
        {
            //log.Debug($"isAuth {isAuth}");
            //log.Debug("Client_BeginReceive");
            // Prevent invalid call..
            if (!this._isRunning)
                return;

            try
            {
                this._clientSocket.BeginReceive(this._clientBuffer, 0, MAX_BUFFER_SIZE, SocketFlags.None,
                    new AsyncCallback(OnClientReceiveData), this._clientSocket);
            }
            catch (SocketException ex)
            {
                log.Debug(ex.ToString());
                this.Stop();
                throw;
            }
            catch (Exception ex)
            {
                log.Debug(ex.ToString());
                this.Stop();
                throw;
            }
        }

        /// <summary>
        /// Begins an async event to receive incoming data.
        /// </summary>
        private void Server_BeginReceive()
        {
            // Prevent invalid call..
            if (!this._isRunning)
                return;

            try
            {
                this._serverSocket.BeginReceive(this._serverBuffer, 0, MAX_BUFFER_SIZE, SocketFlags.None,
                    new AsyncCallback(OnServerReceiveData), this._serverSocket);
            }
            catch (SocketException ex)
            {
                log.Debug(ex.ToString());
                this.Stop();
            }
            catch (Exception ex)
            {
                log.Debug(ex.ToString());
                this.Stop();
            }
        }

        /// <summary>
        /// Completes an async event to receive data.
        /// </summary>
        /// <param name="result"></param>
        private void OnClientReceiveData(IAsyncResult result)
        {
            //log.Debug($"isAuth {isAuth}");
            //log.Debug("OnClientReceiveData");
            // Prevent invalid calls to this function..
            if (!this._isRunning || result.IsCompleted == false || !(result.AsyncState is Socket))
            {
                this.Stop();
                return;
            }

            lock (syncLock)
            {

                Socket client = (result.AsyncState as Socket);

                // Attempt to end the async call..
                int recvCount = 0;
                try
                {
                    try
                    {
                        recvCount = client.EndReceive(result);
                    }
                    catch (Exception exception)
                    {
                        //log.Debug(exception.ToString());
                        this.Stop();
                        throw;
                        // TODO: atleast log the exception.
                    }

                    if (recvCount == 0)
                    {
                        this.Stop();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    this.Stop();
                    return;
                }

                // Read the current packet.
                byte[] recvData = new byte[recvCount];
                Array.Copy(this._clientBuffer, 0, recvData, 0, recvCount);

                this._clientBacklog.AddRange(recvData);

                //this.SendToServerRaw(recvData);

                // Iterate through the whole packets, parse them and delete them from the buffer.
                while (true)
                {
                    /*
                    L2Packet structure:
                    | 2 byte size | 1 byte id | up to 65535 bytes for data (payload) |
                    */

                    // If packet buffer is lesser than 3 bytes there is no valid packet for parsing (the minimum is atleast 3 bytes (2 bytes for size, 1 for id)
                    if (this._clientBacklog.Count < 3)
                        break;

                    while(L2Injector._sgObjInit == false)
                        Thread.Sleep(20);

                    // Calculate the packet length by parsing the first 2 bytes.
                    int packetSize = BitConverter.ToInt16(this._clientBacklog.ToArray(), 0);
                    if (this._clientBacklog.Count < packetSize)
                        break;

                    byte[] receivedPacket = new byte[packetSize];
                    Array.Copy(this._clientBacklog.ToArray(), 0, receivedPacket, 0, packetSize);
                    bool shouldDropPacket = false;

                    this._clientBacklog.RemoveRange(0, packetSize);
                    if (!isAuth && _isInit)
                    {

                        _gamePacketObfuscator.DeobfuscatePacketFromClient(receivedPacket);
                        _secondaryGamePacketObfuscator?.DeobfuscatePacketFromClient(receivedPacket);
                        try
                        {
                            if (!JUSTASNIFFER)
                            {
                                var packet = packetFactory.CreatePacket(receivedPacket, false);
                                packet?.Parse(BotInstance.PlayerData);

                                if (packet?.DropPacket == true && _gamePacketObfuscator is SGInGameCipher)
                                {
                                    shouldDropPacket = true;
                                    //((SGInGameCipher)_gamePacketObfuscator).reverseCrypt(receivedPacket, ((SGInGameCipher)_gamePacketObfuscator).clientKeySend);
                                }
                            }

                            if (JUSTASNIFFER)
                                log.Debug(string.Join(", ", receivedPacket.Select(b => string.Format("{0:X2} ", b))));
                            //PacketParser.HandleOutgoingPacket(receivedPacket, this.data, this.logger);
                            //CB gameguard id
                            //b1 net ping id
                        }
                        catch (Exception ex)
                        {
                            log.Debug(ex.ToString());
                            throw;
                        }

                        if (!shouldDropPacket)
                        {
                            _secondaryGamePacketObfuscator?.ObfuscatePacketForServer(receivedPacket);
                            _gamePacketObfuscator.ObfuscatePacketForServer(receivedPacket);
                        }

                    }
                    else if (!_isInit)
                    {
                        if (_gamePacketObfuscator == null && receivedPacket.Length > 100)
                        {
                            int version = BitConverter.ToInt32(receivedPacket.ToArray(), 3);
                            if (version >= 268 && version <= 272)
                                log.Debug($"Unsupported H5 client.");
                            //log.Debug(version);

                            if (!JUSTASNIFFER)
                            {
                                BotInstance.Init(version, this);
                                packetFactory = ProtocolFactory.CreatePacketFactory(version);
                            }
                        }

                        if (L2Injector.IsSGLoaded() && _gamePacketObfuscator is LegacyInGameCipher)
                        {
                            _gamePacketObfuscator = new SGInGameCipher(new byte[1], 0);
                                //placeholder args, since they are not needed in the alg
                            //if (!JUSTASNIFFER)
                            L2Injector.FillSGObject((SGInGameCipher) _gamePacketObfuscator);
                            //((SGInGameCipher)_gamePacketObfuscator).reverseCrypt(receivedPacket,((SGInGameCipher)_gamePacketObfuscator).clientKeySend);

                            //_gamePacketObfuscator.DeobfuscatePacketFromClient(receivedPacket);
                            _isInit = true;
                        }

                    }
                    //log.Debug("_0");
                    if (_secondaryGamePacketObfuscator == null && L2Injector.IsRpgClient() &&
                        _gamePacketObfuscator is LegacyInGameCipher)
                    {
                        _secondaryGamePacketObfuscator =
                            new RpgInGameCipher(((LegacyInGameCipher) _gamePacketObfuscator).DynamicKeyBytes, 0);
                        ((RpgInGameCipher) _secondaryGamePacketObfuscator).SecretKey = L2Injector.GetRpgSecretKey();
                    }

                    // Send this packet to the server.
                    //log.Debug("_1");
                    if(!shouldDropPacket)
                        this.SendToServerRaw(receivedPacket);
                }

                // Begin listening for the next packet.
                this.Client_BeginReceive();
                //log.Debug("_5");
            }
        }

        /// <summary>
        /// Completes an async event to receive data.
        /// </summary>
        /// <param name="result"></param>
        private void OnServerReceiveData(IAsyncResult result)
        {
            // Prevent invalid calls to this function.
            if (!this._isRunning || result.IsCompleted == false || !(result.AsyncState is Socket))
            {
                this.Stop();
                return;
            }

            Socket server = ((Socket) result.AsyncState);

            // Attempt to end the async call.
            int nRecvCount = 0;
            try
            {
                try
                {
                    nRecvCount = server == null ? 0 : server.EndReceive(result);
                }
                catch
                {
                    // TODO: atleast log the exception.
                }

                if (nRecvCount == 0)
                {
                    this.Stop();
                    return;
                }
            }
            catch (Exception ex)
            {
                log.Debug(ex.ToString());
                this.Stop();
                return;
            }

            // Read the current packet.
            byte[] recvData = new byte[nRecvCount];
            Array.Copy(this._serverBuffer, 0, recvData, 0, nRecvCount);
            this._serverBacklog.AddRange(recvData);

            // Iterate through the whole received stream, identify packets, parse them and delete them from the buffer.
            while (true)
            {
                /*
                L2Packet structure:
                | 2 byte size | 1 byte id | up to 65535 bytes for data (payload) |
                */

                // If packet buffer is lesser than 3 bytes there is no valid packet for parsing (the minimum is atleast 3 bytes (2 bytes for size, 1 for id)
                if (this._serverBacklog.Count < 3)
                    break;

                while (L2Injector._sgObjInit == false)
                    Thread.Sleep(20);

                // Calculate the packet length by parsing the first 2 bytes.
                int nPacketSize = BitConverter.ToUInt16(this._serverBacklog.ToArray(), 0);
                if (this._serverBacklog.Count < nPacketSize)
                    break;

                byte[] receivedPacket = new byte[nPacketSize];
                Array.Copy(this._serverBacklog.ToArray(), 0, receivedPacket, 0, nPacketSize);

                this._serverBacklog.RemoveRange(0, nPacketSize);

                // Send this packet to the client..
                if (isAuth)
                    _blowfishDecypher.decryptBlock(receivedPacket, 2, receivedPacket, 2);
                if (this.isAuth && receivedPacket[2] != 4)
                {
                    _blowfishDecypher.encryptBlock(receivedPacket, 2, receivedPacket, 2);
                    this.SendToClientRaw(receivedPacket);
                }
                else if (isAuth)
                {
                    _blowfishDecypher.encryptBlock(receivedPacket, 2, receivedPacket, 2);
                }

                int asd = 0;
                // TODO: Get rid of this control flow.
                if (isAuth)
                    if (!_isInit)
                    {
                        // Decipher the initial packet with the static blowfish key.
                        _blowfishDecypher.setKey(Blowfish.StaticKey);
                        for (int i = 0; i < receivedPacket.Length - 2; i += 8)
                        {
                            _blowfishDecypher.decryptBlock(receivedPacket, 2 + i, receivedPacket, 2 + i);
                        }

                        // Remove the xor mask.
                        int xorKey = receivedPacket[receivedPacket.Length - 8] |
                                     (receivedPacket[receivedPacket.Length - 7] << 8) |
                                     (receivedPacket[receivedPacket.Length - 6] << 16) |
                                     (receivedPacket[receivedPacket.Length - 5] << 24);
                        Blowfish.RemoveXorMask(receivedPacket, ref xorKey);

                        //Blowfish.ApplyXorMask(receivedPacket, ref xorKey);

                        // Extract the new blowfish key for the rest of the authentification communication.
                        byte[] dynamicKey = new byte[16];
                        Array.Copy(receivedPacket, 155, dynamicKey, 0, 16);

                        // Cypher the packet again.
                        for (int i = 0; i < receivedPacket.Length - 2; i += 8)
                        {
                            _blowfishDecypher.encryptBlock(receivedPacket, 2 + i, receivedPacket, 2 + i);
                        }

                        // Set the newly acquired blowfish key from the server.
                        _blowfishDecypher.setKey(dynamicKey);
                        _isInit = true;
                    }
                    else
                    {
                        for (int i = 0; i < receivedPacket.Length - 2; i += 8)
                        {
                            _blowfishDecypher.decryptBlock(receivedPacket, 2 + i, receivedPacket, 2 + i);
                        }

                        if (receivedPacket[2] == 4)
                        {
                            //Restore default connect procedure, so client can connect to our proxied servers.
                            L2Injector.RestoreHookedBytes();

                            int serverCount = receivedPacket[3];
                            int serverBlock = 21;
                            for (int i = 0; i < serverCount; i++)
                            {
                                var remoteIp = receivedPacket[6 + i*serverBlock] + "." +
                                               receivedPacket[7 + i*serverBlock] + "." +
                                               receivedPacket[8 + i*serverBlock] + "." +
                                               receivedPacket[9 + i*serverBlock];
                                var remotePort = receivedPacket[10 + i*serverBlock] |
                                                 (receivedPacket[11 + i*serverBlock] << 8);

                                bool isHosted = false;
                                int localPort = PortChecker.GetOpenPort();

                                //Check if this game server is already hosted
                                foreach (var gserver in Server.GameServers)
                                {
                                    if (remoteIp == gserver.RemoteAddress && gserver.BotInstance == BotInstance)
                                    {
                                        isHosted = true;
                                        localPort = gserver.LocalPort;
                                    }
                                }

                                if (!isHosted)
                                {
                                    //brazil hack
                                    //if (remoteIp == "181.119.6.72")
                                    //    remotePort = 7779;

                                    //if (remoteIp == "45.126.211.94")
                                    //    remotePort = 7779;

                                    Server gameServer = new Server
                                    {
                                        IsAuthServer = false,
                                        LocalAddress = "127.0.0.1", //localhost
                                        LocalPort = localPort,
                                        //this will result in the default game port (7777) - server id (ex. 16 for naia) = 7761 (thus avoiding conflicts)
                                        RemoteAddress = remoteIp, //current naia game server ip
                                        RemotePort = remotePort, //7777
                                        ServerId = receivedPacket[5 + i*serverBlock],
                                        L2Injector = L2Injector,
                                        BotInstance = this.BotInstance
                                    };
                                    gameServer.Start();

                                    //log.Debug($"{remoteIp}:{remotePort}");
                                    Server.GameServers.Add(gameServer);
                                }

                                // Modifying the packet to point to our proxys.
                                string[] ipParts = "127.0.0.1".Split('.');
                                receivedPacket[6 + i * serverBlock] = byte.Parse(ipParts[0]);
                                receivedPacket[7 + i * serverBlock] = byte.Parse(ipParts[1]);
                                receivedPacket[8 + i * serverBlock] = byte.Parse(ipParts[2]);
                                receivedPacket[9 + i * serverBlock] = byte.Parse(ipParts[3]);
                                receivedPacket[10 + i * serverBlock] = (byte)(localPort & 0xff);
                                receivedPacket[11 + i * serverBlock] = (byte)((localPort >> 8) & 0xff);
                            }
                        }

                        for (int i = 0; i < receivedPacket.Length - 2; i += 8)
                        {
                            _blowfishDecypher.encryptBlock(receivedPacket, 2 + i, receivedPacket, 2 + i);
                        }

                        _blowfishDecypher.decryptBlock(receivedPacket, 2, receivedPacket, 2);

                        if (receivedPacket[2] == 4)
                        {
                            _blowfishDecypher.encryptBlock(receivedPacket, 2, receivedPacket, 2);
                            this.SendToClientRaw(receivedPacket);
                        }

                    }
                else
                {
                    if (!_isInit)
                    {
                        byte[] dynamicKeyBytes = new byte[8];
                        //receivedPacket[4] = 0;
                        //receivedPacket[5] = 0;
                        //receivedPacket[6] = 0;
                        //receivedPacket[7] = 0;
                        //receivedPacket[8] = 0;
                        //receivedPacket[9] = 0;
                        //receivedPacket[10] = 0;
                        //receivedPacket[11] = 0;
                        Array.Copy(receivedPacket, 4, dynamicKeyBytes, 0, 8);

                        //int seed = BitConverter.ToInt32(receivedPacket.ToArray(), 21);
                        if (JUSTASNIFFER)
                        {
                            log.Debug(BitConverter.ToInt64(dynamicKeyBytes, 0));
                            log.Debug(string.Join(", ", dynamicKeyBytes.Select(b => string.Format("{0:X2} ", b))));
                        }

                        _gamePacketObfuscator = new LegacyInGameCipher(dynamicKeyBytes, 0);
                        if (!L2Injector.IsSGLoaded())
                            _isInit = true;
                    }
                    else
                    {
                        lock (syncLock2)
                        {
                            _gamePacketObfuscator.DeobfuscatePacketFromServer(receivedPacket);
                            _secondaryGamePacketObfuscator?.DeobfuscatePacketFromServer(receivedPacket);
                            //log.Debug(string.Join(", ", receivedPacket.Select(b => string.Format("{0:X2} ", b))));
                            //PacketParser.HandleIncomingPacket(receivedPacket, this.data, this.logger);

                            if (!JUSTASNIFFER)
                            {
                                var packet = packetFactory.CreatePacket(receivedPacket, true);
                                packet?.Parse(BotInstance.PlayerData);

                                //if(packet == null && receivedPacket[2] != 74)
                                //log.Debug(receivedPacket[2]);
                            }


                            _secondaryGamePacketObfuscator?.ObfuscatePacketForClient(receivedPacket);
                            _gamePacketObfuscator.ObfuscatePacketForClient(receivedPacket);
                        }
                    }
                }

                if (!this.isAuth && receivedPacket != null)
                {
                    lock (syncLock)
                    {
                        this.SendToClientRaw(receivedPacket);
                    }
                }

            }

            // Begin listening for next packet.
            this.Server_BeginReceive();
        }

        /// <summary>
        /// Sends the given packet data to the client socket.
        /// </summary>
        /// <param name="btPacket"></param>
        public void SendToClientRaw(byte[] btPacket)
        {
            if (!this._isRunning)
                return;

            try
            {
                this._clientSocket.BeginSend(btPacket, 0, btPacket.Length, SocketFlags.None,
                    new AsyncCallback((x) =>
                    {
                        if (x.IsCompleted == false || !(x.AsyncState is Socket) || _clientSocket == null ||
                            _clientSocket.Connected == false || x.AsyncState == null)
                        {
                            this.Stop();
                            return;
                        }

                        (x.AsyncState as Socket).EndSend(x);
                    }), this._clientSocket);
            }
            catch (Exception ex)
            {
                this.Stop();
            }
        }

        /// <summary>
        /// Sends the given packet data to the server socket.
        /// </summary>
        /// <param name="btPacket"></param>
        public void SendToServerRaw(byte[] btPacket)
        {
            if (!this._isRunning)
                return;
            //log.Debug("_2");
            try
            {
                this._serverSocket.BeginSend(btPacket, 0, btPacket.Length, SocketFlags.None,
                    new AsyncCallback((x) =>
                    {
                        if (x.IsCompleted == false || !(x.AsyncState is Socket))
                        {
                            this.Stop();
                            return;
                        }

                        try
                        {
                            (x.AsyncState as Socket).EndSend(x);
                        }
                        catch (Exception)
                        {
                            
                        }
                    }), this._serverSocket);
                //log.Debug("_3");
            }
            catch (ThreadAbortException) { }
            catch (Exception ex) { log.Debug(ex.ToString()); this.Stop(); }
            //log.Debug("_4");
        }

        /// <summary>
        /// Gets the base client socket.
        /// </summary>
        public Socket ClientSocket
        {
            get
            {
                if (this._isRunning && this._clientSocket != null)
                    return this._clientSocket;
                return null;
            }
        }

        /// <summary>
        /// Gets the base server socket.
        /// </summary>
        public Socket ServerSocket
        {
            get
            {
                if (this._isRunning && this._serverSocket != null)
                    return this._serverSocket;
                return null;
            }
        }
    }
}
