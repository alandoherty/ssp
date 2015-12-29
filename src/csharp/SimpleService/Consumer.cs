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
        public void Message(string service, JObject obj) {
            Message(service, obj, "");
        }

        /// <summary>
        /// Sends a request to the host with authentication.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="obj">The JSON.</param>
        /// <param name="token">The token.</param>
        public void Message(string service, JObject obj, string token) {
            // check parameters
            if (token.Length > Packet.TOKEN_SIZE)
                throw new InvalidOperationException("The token cannot be longer than 32 characters");
            else if(token.Length > Packet.SERVICE_SIZE)
                throw new InvalidOperationException("The token cannot be longer than 32 characters");

            // get json
            string json = obj.ToString(Formatting.None);

            // serialize
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            // write packet
            client.Write(Packet.Create(client.Peer, Packet.Opcode.Message, service, token, jsonBytes));
        }

        /// <summary>
        /// Polls the consumer for messages and requests.
        /// </summary>
        public void Poll() {

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
