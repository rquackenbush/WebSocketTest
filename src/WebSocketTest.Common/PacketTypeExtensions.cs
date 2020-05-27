using System;
using System.Net.WebSockets;


namespace WebSocketTest.Common
{
    public static class PacketTypeExtensions
    {
        public static WebSocketMessageType Map(this PacketType packetType)
        {
            switch (packetType)
            {
                case PacketType.Binary:
                    return WebSocketMessageType.Binary;

                case PacketType.Text:
                    return WebSocketMessageType.Text;

                default:
                    throw new NotSupportedException($"Unable to map '{packetType}'.");
            }
        }

        public static PacketType Map(this WebSocketMessageType messageType)
        {
            switch (messageType)
            {
                case WebSocketMessageType.Binary:
                    return PacketType.Binary;

                case WebSocketMessageType.Text:
                    return PacketType.Text;

                default:
                    throw new NotSupportedException($"Unable to map '{messageType}'.");
            }
        }
    }
}
