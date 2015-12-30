using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace SimpleService.Protocol
{
    internal class Transaction
    {
        #region Fields
        private ushort sequence;
        private string service;
        private string token;
        private JObject data;
        private Type type;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the sequence.
        /// </summary>
        public ushort Sequence {
            get {
                return sequence;
            }
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        public string Service {
            get {
                return service;
            }
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        public string Token {
            get {
                return token;
            }
        }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        public JObject Data {
            get {
                return data;
            }
        }

        /// <summary>
        /// Gets if the transaction is a message.
        /// </summary>
        public bool IsMessage {
            get {
                return type == Type.Message;
            }
        }

        /// <summary>
        /// Gets if the transaction is a request.
        /// </summary>
        public bool IsRequest {
            get {
                return type == Type.Request;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a message packet.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="json">The payload.</param>
        /// <param name="token">The token.</param>
        /// <param name="peer">The peer.</param>
        /// <returns></returns>
        internal static Packet CreateMessage(string service, string json, string token, Peer peer) {
            // check parameters
            if (token.Length > Packet.TOKEN_SIZE)
                throw new InvalidOperationException("The token cannot be longer than 32 characters");
            else if (token.Length > Packet.SERVICE_SIZE)
                throw new InvalidOperationException("The token cannot be longer than 32 characters");

            // serialize
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            // write packet
            return Packet.Create(peer, Packet.Opcode.Message, service, token, jsonBytes);
        }

        /// <summary>
        /// Creates a request packet.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="json">The payload.</param>
        /// <param name="token">The token.</param>
        /// <param name="peer">The peer.</param>
        /// <returns></returns>
        internal static Packet CreateRequest(string service, string json, string token, Peer peer) {
            // reply
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            // build packet and send
            return Packet.Create(peer, Packet.Opcode.Request, service, token, jsonBytes);
        }

        /// <summary>
        /// Creates a response packet.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="json">The payload.</param>
        /// <param name="peer">The peer.</param>
        /// <returns></returns>
        internal static Packet CreateResponse(Transaction transaction, string json, Peer peer) {
            // reply
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            // build packet and send
            return Packet.Create(peer, Packet.Opcode.Request, transaction.service, transaction.token, jsonBytes, transaction.sequence);
        }
        #endregion

        #region Enumerations
        /// <summary>
        /// A transaction type.
        /// </summary>
        private enum Type
        {
            Message,
            Request
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a transaction from an incoming packet.
        /// </summary>
        /// <param name="packet">The packet.</param>
        internal Transaction(Packet packet) {
            this.sequence = packet.Sequence;
            this.token = packet.Token;
            this.type = (packet.Type == Packet.Opcode.Message) ? Type.Message : Type.Request;
            this.service = packet.Service;

            // data
            string json = Encoding.UTF8.GetString(packet.Data);

            this.data = JObject.Parse(json);
        }
        #endregion
    }
}
