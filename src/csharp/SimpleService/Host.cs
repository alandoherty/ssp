using Newtonsoft.Json.Linq;
using SimpleService.Protocol;
using System;
using System.Net;

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
