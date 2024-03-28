/******************************************************************************
 *                                  IPK-2024-1
 *                                  UdpJoin.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains class for UdpJoin message
 *                  Last change: 27.03.23
 *****************************************************************************/

using System.Text;
using IPK_2024_1.Inner;

namespace IPK_2024_1.Messages
{
    internal class UdpJoin : UdpMessage
    {
        public void EncodeMessage(string channelId, string displayName)
        {
            try
            {
                ChannelId = channelId;
                DisplayName = displayName;
                MessageId = UdpClientLogic.GetNewId();

                var messageIdSection = BitConverter.GetBytes((ushort)MessageId);
                var channelIdSection = Encoding.ASCII.GetBytes(channelId);
                var displayNameSection = Encoding.ASCII.GetBytes(displayName);

                Data = new byte[messageIdSection.Length + channelIdSection.Length + displayNameSection.Length + 3];
                var index = 0;

                Data[index++] = 0x03;
                Array.Copy(messageIdSection, 0, Data, index, messageIdSection.Length);
                index += messageIdSection.Length;
                Array.Copy(channelIdSection, 0, Data, index, channelIdSection.Length);
                index += channelIdSection.Length;
                Data[index++] = 0x00;
                Array.Copy(displayNameSection, 0, Data, index, displayNameSection.Length);
                index += displayNameSection.Length;
                Data[index] = 0x00;
            }
            catch
            {
                ErrorHandler.Error(ErrorHandler.ErrorType.MessageEncodingError);
            }
        }
        public override void DecodeMessage(byte[] data)
        {
            // There is no need to decode join message, because server never sends it
        }
    }
}
