using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace SimpleService.Protocol
{
    internal class Connection : IClient
    {
        #region Constants
        public const int KEEP_ALIVE_DELAY = 3;
        public const int KEEP_ALIVE_WAIT = KEEP_ALIVE_DELAY * 2;
        #endregion

        #region Fields
        private Peer peer;
        private ConcurrentQueue<Packet> packetsIn;
        private ConcurrentQueue<Packet> packetsOut;
        private long lastSendAlive = Utilities.Timestamp() - KEEP_ALIVE_WAIT;
        private long lastRecvAlive = Utilities.Timestamp() - KEEP_ALIVE_WAIT;
        private Thread thread;
        private Server server;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the underlying peer.
        /// </summary>
        public Peer Peer {
            get {
                return peer;
            }
        }

        /// <summary>
        /// Gets if this connection is currently connected.
        /// </summary>
        public bool Connected {
            get {
                return peer.Connected;
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

        /// <summary>
        /// Gets the parent server.
        /// </summary>
        public Server Server {
            get {
                return server;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close() {
            peer.Close();
        }

        /// <summary>
        /// Disconnects the connection.
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
            Utilities.DebugLog("connection from " + peer.RemoteAddress);

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
                    try {
                        // read packet
                        Packet p = peer.Read();

                        if (p.Type == Packet.Opcode.Internal)
                            ProcessInternal(p);
                        else
                            packetsIn.Enqueue(p);
                    } catch (Exception ex) {
                        Utilities.DebugLog("failed to decode packet: " + ex.Message);
                    }
                }

                // check if keep alive required
                if (lastSendAlive + KEEP_ALIVE_DELAY < Utilities.Timestamp()) {
                    Write(Packet.Create(peer, Packet.Opcode.Internal, "__KeepAlive", "", new byte[] { }));
                    lastSendAlive = Utilities.Timestamp();
                }

                // check for timeout
                if (lastRecvAlive + KEEP_ALIVE_WAIT < Utilities.Timestamp()) {
                    peer.Disconnect("Timeout", true);
                }
            }

            // log
            Utilities.DebugLog("disconnection from " + peer.RemoteAddress + " for " + peer.DisconnectReason);
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
        /// Creates a new connection from a socket.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="client">The client.</param>
        public Connection(Server server, TcpClient client) {
            this.peer = new Peer(client);
            this.packetsIn = new ConcurrentQueue<Packet>();
            this.packetsOut = new ConcurrentQueue<Packet>();
            this.lastRecvAlive = Utilities.Timestamp();

            // thread
            this.thread = new Thread(Loop);
            this.thread.Name = "SSConnection";
            this.thread.IsBackground = true;
            this.thread.Start();
        }
        #endregion
    }
}
