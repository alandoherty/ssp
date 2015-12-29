using Newtonsoft.Json.Linq;
using SimpleService.Protocol;
using System;
using System.Net;
using System.Text;

namespace SimpleService
{
    public class Host
    {
        #region Fields
        private Server server;
        #endregion

        #region Methods
        /// <summary>
        /// Adds a request handler.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="visibility">The visibility.</param>
        /// <param name="handler">The handler.</param>
        public void Add(string name, Visibility visibility, ServiceRequestHandler handler) {
          
        }
        
        /// <summary>
        /// Adds a message handler.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="visibility">The visibility.</param>
        /// <param name="handler">The handler.</param>
        public void Add(string name, Visibility visibility, ServiceMessageHandler handler) {

        }

        /// <summary>
        /// Polls the host for processing.
        /// </summary>
        public void Poll() {
            foreach(Connection conn in server.Connections) {
                while (conn.Available) {
                    // read packet
                    Packet p = conn.Read();

                    // handle
                    if (p.Type == Packet.Opcode.Message || p.Type == Packet.Opcode.Request) {
                        // get json
                        string json = Encoding.UTF8.GetString(p.Data);

                        Utilities.DebugLog("received message " + p.Service + ": " + json);
                    } else {
                        conn.Disconnect("Invalid packet");
                        return;
                    }
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new service host.
        /// </summary>
        /// <param name="port">The port.</param>
        public Host(int port)
            : this(IPAddress.Any, port) { }

        /// <summary>
        /// Creates a new service host.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public Host(IPAddress address, int port) {
            this.server = new Server(address, port);
            this.server.Start();
        }
        #endregion
    }
}
