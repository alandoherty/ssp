using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SimpleService.Protocol
{
    internal class Peer
    {
        #region Fields
        private TcpClient client;
        private NetworkStream stream;
        private ushort sequence;
        private State state;
        private string localAddress;
        private string remoteAddress;
        private string disconnectReason;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the string representation of the remote address.
        /// </summary>
        public string RemoteAddress {
            get {
                return remoteAddress;
            }
        }

        /// <summary>
        /// Gets the string representation of the local address.
        /// </summary>
        public string LocalAddress {
            get {
                return localAddress;
            }
        }

        /// <summary>
        /// Gets if the local peer is connected to the remote peer.
        /// </summary>
        public bool Connected {
            get {
                return client.Connected && state == State.Connected;
            }
        }

        /// <summary>
        /// Gets the underlying client.
        /// </summary>
        public TcpClient Client {
            get {
                return client;
            }
        }
            
        /// <summary>
        /// Gets the underlying stream.
        /// </summary>
        public NetworkStream Stream {
            get {
                return stream;
            }
        }

        /// <summary>
        /// Gets if packets are available.
        /// </summary>
        public bool Available {
            get {
                if (!Connected)
                    return false;

                return client.Client.Available > 0;
            }
        }

        /// <summary>
        /// Gets the next sequence number.
        /// </summary>
        public ushort Sequence {
            get {
                return sequence;
            }
        }

        /// <summary>
        /// Gets the disconnect reason, null if none available.
        /// </summary>
        public string DisconnectReason {
            get {
                return disconnectReason;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads a packet.
        /// </summary>
        /// <returns>The packet.</returns>
        public Packet Read() {
            try {
                return Packet.Deserialize(this, stream);
            } catch (ProtocolException ex) {
                Disconnect(ex.Message, false);
            
                return null;
            } catch (Exception) {
                Disconnect("Socket read error", false);
                return null;
            }
        }

        /// <summary>
        /// Writes a packet.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public void Write(Packet packet) {
            try {
                packet.Serialize(stream);
            } catch (Exception) {
                Disconnect("Socket write error", false);
                return;
            }

            // increment sequence value
            if (sequence == ushort.MaxValue)
                sequence = 0;
            else
                sequence++;
        }

        /// <summary>
        /// Disconnects the peer, optionally sending the reason to the remote peer.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <param name="send">If a disconnect packet should be sent.</param>
        public void Disconnect(string reason, bool send=true) {
            // state
            state = State.Disconnecting;

            // build packet
            if (send) {
                using (MemoryStream ms = new MemoryStream()) {
                    // write the reason
                    ms.Write(Packet.EncodeFixedBytes(reason, 255), 0, 255);

                    // write packet
                    Write(Packet.Create(this, Packet.Opcode.Internal, "__Disconnect", "", ms.ToArray(), sequence));
                }
            }

            // reason
            if (disconnectReason == null)
                disconnectReason = reason;

            // close
            Close();
        }

        /// <summary>
        /// Closes the peer.
        /// </summary>
        public void Close() {
            try {
                state = State.Disconnected;
                client.Close();
            } catch(Exception) {
            }
        }

        /// <summary>
        /// Flushes all packets to the peer.
        /// </summary>
        public void Flush() {
            stream.Flush();
        }
        #endregion

        #region Enums
        /// <summary>
        /// The state of the peer.
        /// </summary>
        public enum State
        {
            Connected,
            Disconnecting,
            Disconnected
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new peer from a tcp client.
        /// </summary>
        /// <param name="client">The client.</param>
        public Peer(TcpClient client) {
            this.client = client;
            this.client.ReceiveTimeout = 3000;
            this.client.SendTimeout = 3000;
            this.stream = client.GetStream();
            this.state = State.Connected;
            this.localAddress = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
            this.disconnectReason = null;
            this.remoteAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        }
        #endregion
    }
}
