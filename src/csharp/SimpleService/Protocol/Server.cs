using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SimpleService.Protocol
{
    internal class Server
    {
        #region Fields
        private int port;
        private TcpListener listener;
        private List<Connection> connections;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the port.
        /// </summary>
        public int Port {
            get {
                return port;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts listening.
        /// </summary>
        public void Start() {
            // start the listener
            listener.Start();

            // setup handlers
            listener.BeginAcceptTcpClient(Accept, null);
        }

        /// <summary>
        /// Polls the server.
        /// </summary>
        public void Poll() {
            
        }

        /// <summary>
        /// Accepts a client.
        /// </summary>
        /// <param name="res">The async result object.</param>
        private void Accept(IAsyncResult res) {
            // end accept
            TcpClient client = listener.EndAcceptTcpClient(res);

            // create connection
            Connection conn = new Connection(client);

            // push to connections
            connections.Add(conn);
        }

        /// <summary>
        /// Stops listening.
        /// </summary>
        public void Stop() {
            // stops the listener
            listener.Stop();
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new server on the specified port.
        /// </summary>
        /// <param name="port">The port.</param>
        public Server(int port)
            : this(IPAddress.Any, port) { }

        /// <summary>
        /// Creates a new server on the specified address and port.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public Server(IPAddress address, int port) {
            this.port = port;
            this.listener = new TcpListener(address, port);
            this.connections = new List<Connection>();
        }
        #endregion
    }
}
