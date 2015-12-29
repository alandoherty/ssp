using System;
using System.IO;
using System.Text;

namespace SimpleService.Protocol
{
    internal class Packet
    {
        #region Constants
        /// <summary>
        /// The magic prefixing all packets.
        /// </summary>
        public const string MAGIC = "JSPKZ";

        /// <summary>
        /// The maximum size of an authenticating token.
        /// </summary>
        public const int TOKEN_SIZE = 32;

        /// <summary>
        /// The maximum size of a service string.
        /// </summary>
        public const int SERVICE_SIZE = 48;

        /// <summary>
        /// The total size of a packet header.
        /// </summary>
        public static int HEADER_SIZE = TOKEN_SIZE + SERVICE_SIZE + sizeof(byte) + sizeof(int) + sizeof(ushort) + sizeof(uint) + MAGIC.Length;
        #endregion

        #region Fields
        private Opcode opcode;
        private ushort sequence;
        private string token;
        private string service;
        private byte[] data;
        private Peer source;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the packet opcode.
        /// </summary>
        public Opcode Type {
            get {
                return opcode;
            }
        }

        /// <summary>
        /// Gets the peer the packet came from.
        /// </summary>
        public Peer Source {
            get {
                return source;
            }
        }

        /// <summary>
        /// Gets the sequence.
        /// </summary>
        public ushort Sequence {
            get {
                return sequence;
            }
        }

        /// <summary>
        /// Gets the authentication token.
        /// </summary>
        public string Token {
            get {
                return token;
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
        /// Gets the packet payload.
        /// </summary>
        public byte[] Data {
            get {
                return data;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Serializes the packet to the stream.
        /// </summary>
        public void Serialize(Stream stream) {
            // writer
            BinaryWriter writer = new BinaryWriter(stream);

            // magic
            writer.Write(Encoding.ASCII.GetBytes(MAGIC), 0, MAGIC.Length);

            // opcode, length & sequence
            writer.Write((byte)opcode);
            writer.Write(data.Length);
            writer.Write(sequence);

            // unused
            writer.Write((uint)0);

            // token
            writer.Write(EncodeFixedBytes(token, TOKEN_SIZE));

            // service
            writer.Write(EncodeFixedBytes(service, SERVICE_SIZE));

            // data
            writer.Write(data);
        }

        /// <summary>
        /// Creates a new packet from a buffer.
        /// </summary>
        /// <param name="peer">The peer.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static Packet Deserialize(Peer peer, Stream stream) {
            // create packet
            Packet p = new Packet();
            p.source = peer;

            // reader
            BinaryReader reader = new BinaryReader(stream);

            // magic
            string magic = Encoding.ASCII.GetString(reader.ReadBytes(MAGIC.Length));

            if (magic != MAGIC)
                throw new ProtocolException("Incoming packet has invalid magic", ProtocolError.InvalidMagic);

            // opcode, length & sequence
            p.opcode = (Opcode)reader.ReadByte();
            p.data = new byte[reader.ReadInt32()];
            p.sequence = reader.ReadUInt16();

            // unused
            reader.ReadUInt32();

            // token
            p.token = DecodeFixedBytes(reader.ReadBytes(TOKEN_SIZE), TOKEN_SIZE, TOKEN_SIZE);

            // service
            p.service = DecodeFixedBytes(reader.ReadBytes(SERVICE_SIZE), SERVICE_SIZE / 2, SERVICE_SIZE);

            // data
            p.data = reader.ReadBytes(p.data.Length);

            return p;
        }

        /// <summary>
        /// Decodes a fixed byte array into an ASCII string.
        /// </summary>
        /// <param name="fixedStr">The fixed string.</param>
        /// <param name="capacity">The capacity.</param>
        /// <param name="maxCapacity">The maximum capacity.</param>
        /// <returns></returns>
        internal static string DecodeFixedBytes(byte[] fixedStr, int capacity, int maxCapacity) {
            StringBuilder str = new StringBuilder(capacity, maxCapacity);

            for (int i = 0; i < TOKEN_SIZE; i++) {
                if (fixedStr[i] != '\0')
                    str.Append((char)fixedStr[i]);
                else
                    break;
            }

            return str.ToString();
        }

        /// <summary>
        /// Encodes a string into a fixed byte array.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="fixedSize">The fixed size.</param>
        /// <returns></returns>
        internal static byte[] EncodeFixedBytes(string str, int fixedSize) {
            byte[] strBytes = new byte[fixedSize];

            // copy
            Array.Copy(Encoding.ASCII.GetBytes(str), strBytes, str.Length);

            return strBytes;
        }

        /// <summary>
        /// Creates a new packet for the specified peer.
        /// </summary>
        /// <param name="peer">The peer.</param>
        /// <param name="opcode">The opcode.</param>
        /// <param name="service">The service.</param>
        /// <param name="token">The authentication token.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static Packet Create(Peer peer, Opcode opcode, string service, string token, byte[] data) {
            Packet p = new Packet();
            p.opcode = opcode;
            p.token = token;
            p.service = service;
            p.sequence = peer.Sequence;
            p.data = data;
            p.source = peer;
            return p;
        }

        /// <summary>
        /// Creates a new packet for the specified peer.
        /// </summary>
        /// <param name="peer">The peer.</param>
        /// <param name="opcode">The opcode.</param>
        /// <param name="service">The service.</param>
        /// <param name="token">The authentication token.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static Packet Create(Peer peer, Opcode opcode, string service, string token, byte[] data, ushort sequence) {
            Packet p = new Packet();
            p.opcode = opcode;
            p.token = token;
            p.service = service;
            p.sequence = sequence;
            p.data = data;
            p.source = peer;
            return p;
        }
        #endregion

        #region Enums
        /// <summary>
        /// The opcodes.
        /// </summary>
        public enum Opcode : byte
        {
            Internal = 0,
            Message = 1,
            Request = 2
        }
        #endregion

        #region Constructors
        private Packet() {
        }
        #endregion
    }
}
