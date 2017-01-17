using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleService.Protocol;
using System;
using System.Collections.Generic;
using System.Net;

namespace SimpleService
{
    public class Consumer
    {
        #region Fields
        private Client client;
        private Dictionary<string, Handler> messageHandlers;
        private Dictionary<string, Handler> requestHandlers;
        private Dictionary<ushort, ServiceResponseHandler> responseHandlers;
        private bool connected = false;
        #endregion

        #region Properties
        /// <summary>
        /// Gets if the consumer is connected to a host.
        /// </summary>
        public bool Connected {
            get {
                return connected;
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
            connected = true;
        }

        /// <summary>
        /// Connects to the specified address and port.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public void Connect(IPAddress address, int port) {
            client.Connect(address, port);
            connected = true;
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
        /// Sends a message to the host.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The object.</param>
        public void Message(string service, object obj) {
            Message(service, obj, "");
        }

        /// <summary>
        /// Sends a message to the host.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The object.</param>
        public void Message(string service, JObject obj) {
            Message(service, obj, "");
        }

        /// <summary>
        /// Sends a message to the host.
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
                throw new InvalidOperationException("The token cannot be longer than " + Packet.TOKEN_SIZE + " characters");
            else if (service.Length > Packet.SERVICE_SIZE)
                throw new InvalidOperationException("The service cannot be longer than " + Packet.SERVICE_SIZE + " characters");

            // write packet
            client.Write(Transaction.CreateMessage(service, json, token, client.Peer));
        }

        /// <summary>
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
        /// Sends a request to the host.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The object.</param>
        /// <param name="handler">The handler.</param>
        public void Request(string service, object obj, ServiceResponseHandler handler) {
            Request(service, obj, handler, "");
        }

        /// <summary>
        /// Sends a request to the host.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The object.</param>
        /// <param name="handler">The handler.</param>
        public void Request(string service, JObject obj, ServiceResponseHandler handler) {
            Request(service, obj, handler, "");
        }

        /// <summary>
        /// Sends a request to the host.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="json">The json.</param>
        /// <param name="handler">The handler.</param>
        public void Request(string service, string json, ServiceResponseHandler handler) {
            Request(service, json, handler, "");
        }

        /// <summary>
        /// Sends a request to the host, serializing the object to JSON and authenticating 
        /// with the provided token.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The object.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="token">The token.</param>
        public void Request(string service, object obj, ServiceResponseHandler handler, string token) {
            Request(service, JsonConvert.SerializeObject(obj, Formatting.None), handler, token);
        }

        /// <summary>
        /// Sends a message to the host, authenticating with the provided token.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The object.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="token">The token.</param>
        public void Request(string service, JObject obj, ServiceResponseHandler handler, string token) {
            Request(service, obj.ToString(Formatting.None), handler, token);
        }

        /// <summary>
        /// Sends a message to the host, authenticating with the provided token.
        /// </summary
        /// <param name="service">The service.</param>
        /// <param name="json">The json.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="token">The token.</param>
        public void Request(string service, string json, ServiceResponseHandler handler, string token) {
            // check parameters
            if (token.Length > Packet.TOKEN_SIZE)
                throw new InvalidOperationException("The token cannot be longer than " + Packet.TOKEN_SIZE + " characters");
            else if (service.Length > Packet.SERVICE_SIZE)
                throw new InvalidOperationException("The service cannot be longer than " + Packet.SERVICE_SIZE + " characters");

            // build request
            Packet req = Transaction.CreateRequest(service, json, token, client.Peer);

            // setup response
            responseHandlers[req.Sequence] = handler;

            // write packet
            client.Write(req);
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
                } else if (p.Type == Packet.Opcode.Disconnect) {
                    connected = false;
                    Utilities.DebugLog("disconnected from " + client.Peer.RemoteAddress + " for " + client.Peer.DisconnectReason);
                    return;
                } else {
                    client.Disconnect("Invalid packet");
                }
            }
        }

        /// <summary>
        /// Handles an incoming message.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        private void HandleMessage(Transaction transaction) {
            if (messageHandlers.ContainsKey(transaction.Service)) {
                Handler handler = messageHandlers[transaction.Service];

                // call
                ((ServiceMessageHandler)handler.Action)(transaction.Data);
            } else {
                client.Disconnect("Service not found");
            }
        }

        /// <summary>
        /// Handles an incoming request.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        private void HandleRequest(Transaction transaction) {
            // call response handler if found
            if (responseHandlers.ContainsKey(transaction.Sequence)) {
                responseHandlers[transaction.Sequence](transaction.Data);
                responseHandlers.Remove(transaction.Sequence);
                return;
            }

            // check for a request handler
            if (requestHandlers.ContainsKey(transaction.Service)) {
                Handler handler = requestHandlers[transaction.Service];

                // call
                object response = ((ServiceRequestHandler)handler.Action)(transaction.Data);

                // reply
                client.Write(Transaction.CreateResponse(transaction, JsonConvert.SerializeObject(response, Formatting.None), client.Peer));
            } else {
                // disconnect
                client.Disconnect("Service not found");
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new consumer.
        /// </summary>
        public Consumer() {
            client = new Client();
            messageHandlers = new Dictionary<string, Handler>();
            requestHandlers = new Dictionary<string, Handler>();
            responseHandlers = new Dictionary<ushort, ServiceResponseHandler>();
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
        public Consumer(IPAddress address, int port) 
            : this() {
            Connect(address, port);
        }
        #endregion
    }
}
