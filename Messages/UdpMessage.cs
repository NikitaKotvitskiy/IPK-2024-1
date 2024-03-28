/******************************************************************************
 *                                  IPK-2024-1
 *                                UdpMessage.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains an abstract class for 
 *                               UdpMessages
 *                  Last change: 27.03.23
 *****************************************************************************/

namespace IPK_2024_1.Messages
{
    internal abstract class UdpMessage
    {
        public byte[]? Data { get; protected set; }             // Data array with encripted message data

        // All possible message parameters
        public ushort MessageId { get; protected set; }
        public ushort? RefMessageId { get; protected set; }
        public string? Username { get; protected set; }
        public string? ChannelId { get; protected set; }
        public string? Secret { get; protected set; }
        public string? DisplayName { get; protected set; }
        public string? MessageContent { get; protected set; }
        public bool? Result { get; protected set; }

        public abstract void DecodeMessage(byte[] data);    // This method implements message decoding

        // This method defines the length of string in byte array (by finging zero byte)
        protected int DefineStringLength(byte[] data, ref int startPosition)
        {
            var length = 0;
            while (data[startPosition] != 0)
            {
                length++;
                startPosition++;
            }
            startPosition++;
            return length;
        }
    }
}
