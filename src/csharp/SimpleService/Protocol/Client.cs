using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleService.Protocol
{
    internal class Client : IClient
    {
        #region Fields
        private Peer peer;
        private Thread thread;
        private ConcurrentQueue<Packet> packetsIn = null;
        private ConcurrentQueue<Packet> packetsOut = null;
        private long lastSendAlive = Utilities.Timestamp() - Connection.KEEP_ALIVE_WAIT;
        private long lastRecvAlive = Utilities.Timestamp() - Connection.KEEP_ALIVE_WAIT;
        #endregion

        #region Properties
        /// <summary>
        /// Gets if this connection is currently connected.
        /// </summary>
        public bool Connected {
            get {
                return peer.Connected;
            }
        }

        /// <summary>
        /// Gets the underlying peer.
        /// </summary>
        public Peer Peer {
            get {
                return peer;
            }
        }

        /// <summary>
        /// Gets if packets are available to read.
        /// </summary>
        public bool Available {
            get {
                return packetsIn.Count > 0;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Connects with the provided client.
        /// </summary>
        /// <param name="client">The client.</param>
        private void Connect(TcpClient client) {
            this.peer = new Peer(client);
            this.lastRecvAlive = Utilities.Timestamp();
            this.thread = new Thread(new ThreadStart(Loop));
            this.thread.IsBackground = true;
            this.thread.Name = "SSClient";
            this.thread.Start();
        }

        /// <summary>
        /// Connects to the specified host and port.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public void Connect(string host, int port) {
            // open connection
            TcpClient cl = new TcpClient();
            cl.Connect(host, port);

            // setup connection
            Connect(cl);
        }

        /// <summary>
        /// Connects to the specified address and port.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public void Connect(IPAddress address, int port) {
            // open connection
            TcpClient cl = new TcpClient();
            cl.Connect(address, port);

            // setup connection
            Connect(cl);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close() {
            peer.Close();
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public void Disconnect(string reason) {
            peer.Disconnect(reason, true);
        }

        /// <summary>
        /// The loop to read/write packets.
        /// </summary>
        private void Loop() {
            // log
            Utilities.DebugLog("connected to " + peer.RemoteAddress);

            // do loop
            while (peer.Connected) {
                // write all packets
                while (packetsOut.Count > 0) {
                    Packet p = null;

                    if (packetsOut.TryDequeue(out p))
                        peer.Write(p);
                }

                // flush
                peer.Flush();

                // process incoming
                while (peer.Available) {
#if !DEBUG
                    try {
#endif
                    // read packet
                    Packet p = peer.Read();

                        if (p == null) {
                            peer.Disconnect("Failed to read packet", false);
                            break;
                        }

                        if (p.Type == Packet.Opcode.Internal)
                            ProcessInternal(p);
                        else
                            packetsIn.Enqueue(p);
#if !DEBUG
                    } catch (Exception ex) {
                        Utilities.DebugLog("bad packet received: " + ex.Message);
                    }
#endif
                }

                // check if keep alive required
                if (lastSendAlive + Connection.KEEP_ALIVE_DELAY < Utilities.Timestamp()) {
                    Write(Packet.Create(peer, Packet.Opcode.Internal, "__KeepAlive", "", new byte[] { }));
                    lastSendAlive = Utilities.Timestamp();
                }

                // check for timeout
                if (lastRecvAlive + Connection.KEEP_ALIVE_WAIT < Utilities.Timestamp()) {
                    peer.Disconnect("Timeout", true);
                }
            }

            // generic disconnect incase it's not picked
            // up elsewhere
            peer.Disconnect("Disconnected");

            // write disconnect opcode
            packetsIn.Enqueue(Packet.Create(peer, Packet.Opcode.Disconnect, "", "", null));
        }

        /// <summary>
        /// Processes an internal packet.
        /// </summary>
        /// <param name="p">The packet.</param>
        private void ProcessInternal(Packet p) {
            if (p.Service == "__KeepAlive") {
                lastRecvAlive = Utilities.Timestamp();
            } else if (p.Service == "__Disconnect") {
                if (p.Data.Length != 255) {
                    peer.Disconnect("Disconnected by server");
                } else {
                    // decode reason
                    string str = Packet.DecodeFixedBytes(p.Data, 32, 255);

                    // do disconnect
                    peer.Disconnect(str);
                }
            } else {
                peer.Disconnect("Invalid internal service", true);
            }
        }

        /// <summary>
        /// Writes a packet to the connected client.
        /// </summary>
        /// <param name="p">The packet.</param>
        public void Write(Packet p) {
            packetsOut.Enqueue(p);
        }

        /// <summary>
        /// Reads a packet from the connected client.
        /// </summary>
        public Packet Read() {
            Packet p = null;
            packetsIn.TryDequeue(out p);
            return p;
        }
#endregion

#region Constructors
        /// <summary>
        /// Creates a new client.
        /// </summary>
        public Client() {
            this.packetsIn = new ConcurrentQueue<Packet>();
            this.packetsOut = new ConcurrentQueue<Packet>();
        }

        /// <summary>
        /// Creates a new client and connects to the specified host and port.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public Client(string host, int port) 
            : this() {
            Connect(host, port);
        }

        /// <summary>
        /// Creates a new client and connects to the specified address and port.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public Client(IPAddress address, int port) 
            : this() {
            Connect(address, port);
        }
#endregion
    }
}
