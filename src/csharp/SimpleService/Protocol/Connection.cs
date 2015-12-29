using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace SimpleService.Protocol
{
    internal class Connection
    {
        #region Fields
        private Peer peer;
        private ConcurrentQueue<Packet> packetsIn;
        private ConcurrentQueue<Packet> packetsOut;
        private Thread thread;
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

                        // queue
                        packetsIn.Enqueue(p);
                    } catch (Exception ex) {
                        Utilities.DebugLog("bad packet received: " + ex.Message);
                    }
                }
            }

            // log
            Utilities.DebugLog("disconnection from " + peer.RemoteAddress);
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
        /// <param name="client">The client.</param>
        public Connection(TcpClient client) {
            this.peer = new Peer(client);
            this.packetsIn = new ConcurrentQueue<Packet>();
            this.packetsOut = new ConcurrentQueue<Packet>();

            // thread
            this.thread = new Thread(Loop);
            this.thread.Name = "SSConnection";
            this.thread.IsBackground = true;
            this.thread.Start();
        }
        #endregion
    }
}
