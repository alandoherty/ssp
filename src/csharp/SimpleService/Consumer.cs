using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleService.Protocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleService
{
    public class Consumer
    {
        #region Fields
        private Client client;
        #endregion

        #region Properties
        /// <summary>
        /// Gets if the consumer is connected to a host.
        /// </summary>
        public bool Connected {
            get {
                return client != null && client.Connected;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Connects to the specified host and port.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public void Connect(string host, int port) {
            client.Connect(host, port);
        }

        /// <summary>
        /// Connects to the specified address and port.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public void Connect(IPAddress address, int port) {
            client.Connect(address, port);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close() {
            client.Close();
        }

        /// <summary>
        /// Disconnects from the host.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public void Disconnect(string reason) {
            client.Disconnect(reason);
        }

        /// <summary>
        /// Sends a request to the host.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The object.</param>
        public void Message(string service, object obj) {
            Message(service, obj, "");
        }

        /// <summary>
        /// Sends a request to the host.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The object.</param>
        public void Message(string service, JObject obj) {
            Message(service, obj, "");
        }

        /// <summary>
        /// Sends a request to the host.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="json">The JSON string..</param>
        public void Message(string service, string json) {
            Message(service, json, "");
        }
        
        /// <summary>
        /// Sends a message to the host, authenticating with the provided token.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="json">The JSON string.</param>
        /// <param name="token">The authentication token.</param>
        public void Message(string service, string json, string token) {
            // check parameters
            if (token.Length > Packet.TOKEN_SIZE)
                throw new InvalidOperationException("The token cannot be longer than 32 characters");
            else if (token.Length > Packet.SERVICE_SIZE)
                throw new InvalidOperationException("The token cannot be longer than 32 characters");

            // write packet
            client.Write(Transaction.CreateMessage(service, json, token, client.Peer));
        }

        /// Sends a message to the host, serializing the object to JSON and authenticating 
        /// with the provided token.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The object.</param>
        /// <param name="token">The authentication token.</param>
        public void Message(string service, object obj, string token) {
            Message(service, JsonConvert.SerializeObject(obj, Formatting.None), token);
        }

        /// <summary>
        /// Sends a message to the host, authenticating with the provided token.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The object.</param>
        /// <param name="token">The token.</param>
        public void Message(string service, JObject obj, string token) {
            Message(service, obj.ToString(Formatting.None), token);
        }

        /// <summary>
        /// Polls and processing messages.
        /// </summary>
        public void Poll() {
            while(client.Available) {
                // read packet
                Packet p = client.Read();

                // handle
                if (p.Type == Packet.Opcode.Message || p.Type == Packet.Opcode.Request) {
                    // get transaction
                    Transaction transaction = null;

                    // try and parse json
                    try {
                        transaction = new Transaction(p);
                    } catch (Exception) {
                        client.Disconnect("Malformed or invalid JSON");

                        continue;
                    }

                    // call handler
                    if (transaction.IsMessage)
                        HandleMessage(transaction);
                    else if (transaction.IsRequest)
                        HandleRequest(transaction);
                }  else {
                    client.Disconnect("Invalid packet");
                    return;
                }
            }
        }

        /// <summary>
        /// Handles an incoming message.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        private void HandleMessage(Transaction transaction) {
            /*
            if (messageHandlers.ContainsKey(transaction.Service)) {
                Handler handler = messageHandlers[transaction.Service];

                // call
                ((ServiceMessageHandler)handler.Action)(transaction.Data);
            } else {
                if (!kickNotFound)
                    return;

                // disconnect
                conn.Disconnect("Service not found");
            }*/
        }

        /// <summary>
        /// Handles an incoming request.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        private void HandleRequest(Transaction transaction) {
            /*
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
            }*/
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new consumer.
        /// </summary>
        public Consumer() {
            client = new Client();
        }

        /// <summary>
        /// Creates a new consumer and connects to the host at the specified host and port.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public Consumer(string host, int port) 
            : this() {
            Connect(host, port);
        }

        /// <summary>
        /// Creates a new consumer and connects to the host at the specified address and port.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public Consumer(IPAddress address, int port) {
            Connect(address, port);
        }
        #endregion
    }
}
