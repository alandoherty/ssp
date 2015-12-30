using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleService.Protocol
{
    internal class Server
    {
        #region Fields
        private int port;
        private TcpListener listener;
        private List<Connection> connections;
        private bool running;
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

        /// <summary>
        /// Gets if the server is running.
        /// </summary>
        public bool Running {
            get {
                return running;
            }
        }

        /// <summary>
        /// Gets the connections.
        /// </summary>
        public List<Connection> Connections {
            get {
                return connections;
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

            running = true;

            // setup handlers
            listener.BeginAcceptTcpClient(Accept, null);
        }

        /// <summary>
        /// Accepts a client.
        /// </summary>
        /// <param name="res">The async result object.</param>
        private void Accept(IAsyncResult res) {
            // end accept
            TcpClient client = listener.EndAcceptTcpClient(res);

            // create connection
            Connection conn = new Connection(this, client);

            // push to connections
            lock(connections) {
                connections.Add(conn);
            }

            // reinstate handlers
            listener.BeginAcceptTcpClient(Accept, null);
        }

        /// <summary>
        /// Stops listening.
        /// </summary>
        public void Stop() {
            // stops the listener
            listener.Stop();

            running = false;
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
