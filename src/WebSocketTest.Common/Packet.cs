namespace WebSocketTest.Common
{
    /// <summary>
    /// A message from a connection.
    /// </summary>
    public class Packet
    {
        //TODO: Determine if we should instead work with ReadOnlyMemory<byte> instead of byte[]
        public Packet(byte[] body, PacketType type)
        {
            Type = type;
            Body = body;
        }

        public PacketType Type { get; }

        public byte[] Body { get; }
    }
}
