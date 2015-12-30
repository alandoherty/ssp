using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleService.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SimpleService
{
    public class Host
    {
        #region Fields
        private Server server;
        private Dictionary<string, Handler> messageHandlers;
        private Dictionary<string, Handler> requestHandlers;
        private bool kickNotFound;
        private bool kickBadJSON;
        #endregion

        #region Properties
        #endregion

        #region Methods
        /// <summary>
        /// Binds a request handler.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="visibility">The visibility.</param>
        /// <param name="handler">The handler.</param>
        public void Bind(string service, Visibility visibility, ServiceRequestHandler handler) {
            // check if key already exists for message
            if (messageHandlers.ContainsKey(service))
                throw new Exception("The service is already binded to " + messageHandlers[service].Action.ToString());

            // add request handler
            this.requestHandlers.Add(service, new Handler() {
                Visibility = visibility,
                Service = service,
                Action = handler
            });
        }

        /// <summary>
        /// Binds a message handler.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="visibility">The visibility.</param>
        /// <param name="handler">The handler.</param>
        public void Bind(string service, Visibility visibility, ServiceMessageHandler handler) {
            // check if the key already exists for request
            if (requestHandlers.ContainsKey(service))
                throw new Exception("The service is already binded to " + requestHandlers[service].Action.ToString());

            // add message handler
            this.messageHandlers.Add(service, new Handler() {
                Visibility = visibility,
                Service = service,
                Action = handler
            });
        }

        /// <summary>
        /// Unbinds a handler.
        /// </summary>
        /// <param name="service">The service.</param>
        public void Unbind(string service) {
            if (requestHandlers.ContainsKey(service))
                requestHandlers.Remove(service);
            else if (messageHandlers.ContainsKey(service))
                messageHandlers.Remove(service);
            else
                throw new Exception("The service cannot be binded as no bind exists");
        }

        /// <summary>
        /// Broadcasts a message to all consumers.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="data">The payload.</param>
        /// <param name="token">The authentication token.</param>
        public void Broadcast(string service, JObject data, string token) {
            foreach(Connection conn in server.Connections) {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Polls the host for processing.
        /// </summary>
        public void Poll() {
            // disconneccted clients
            List<Connection> disconnected = new List<Connection>();

            // process each connection
            lock (server.Connections) {
                foreach (Connection conn in server.Connections) {
                    while (conn.Available) {
                        // check if still connected
                        if (!conn.Connected) {
                            disconnected.Add(conn);
                            break;
                        }

                        // read packet
                        Packet p = conn.Read();

                        // handle
                        if (p.Type == Packet.Opcode.Message || p.Type == Packet.Opcode.Request) {
                            // get transaction
                            Transaction transaction = null;

                            // try and parse json
                            try {
                                transaction = new Transaction(p);
                            } catch (Exception) {
                                if (kickBadJSON)
                                    conn.Disconnect("Malformed or invalid JSON");

                                continue;
                            }

                            // call handler
                            if (transaction.IsMessage)
                                HandleMessage(transaction, conn);
                            else if (transaction.IsRequest)
                                HandleRequest(transaction, conn);
                        } else {
                            conn.Disconnect("Invalid packet");
                            return;
                        }
                    }
                }
            }

            // remove all disconnected
            foreach (Connection conn in disconnected)
                server.Connections.Remove(conn);
        }

        /// <summary>
        /// Handles an incoming message.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="conn">The connection.</param>
        private void HandleMessage(Transaction transaction, Connection conn) {
            if (messageHandlers.ContainsKey(transaction.Service)) {
                Handler handler = messageHandlers[transaction.Service];

                // call
                ((ServiceMessageHandler)handler.Action)(transaction.Data);
            } else {
                if (!kickNotFound)
                    return;

                // disconnect
                conn.Disconnect("Service not found");
            }
        }

        /// <summary>
        /// Handles an incoming request.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="conn">The connection.</param>
        private void HandleRequest(Transaction transaction, Connection conn) {
            if (requestHandlers.ContainsKey(transaction.Service)) {
                Handler handler = requestHandlers[transaction.Service];

                // call
                JObject response = ((ServiceRequestHandler)handler.Action)(transaction.Data);

                // reply
                conn.Write(Transaction.CreateResponse(transaction, response.ToString(Formatting.None), conn.Peer));
            } else {
                if (!kickNotFound)
                    return;

                // disconnect
                conn.Disconnect("Service not found");
            }
        }
        #endregion

        #region Structures
        /// <summary>
        /// A handler entry.
        /// </summary>
        private struct Handler
        {
            public Visibility Visibility;
            public string Service;
            public Delegate Action;
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
            this.requestHandlers = new Dictionary<string, Handler>();
            this.messageHandlers = new Dictionary<string, Handler>();
            this.server = new Server(address, port);
            this.server.Start();
            this.kickNotFound = true;
            this.kickBadJSON = true;
        }
        #endregion
    }
}
