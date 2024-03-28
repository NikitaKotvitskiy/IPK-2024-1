/******************************************************************************
 *                                  IPK-2024-1
 *                                  UdpBye.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains class for UdpBye message
 *                  Last change: 27.03.23
 *****************************************************************************/

using IPK_2024_1.Inner;

namespace IPK_2024_1.Messages
{
    internal class UdpBye : UdpMessage
    {
        public void EncodeMessage()
        {
            try
            {
                MessageId = UdpClientLogic.GetNewId();
                var messageIdSection = BitConverter.GetBytes((ushort)MessageId);
                Data = new byte[messageIdSection.Length + 1];
                var index = 0;
                Data[index++] = 0xFF;
                Array.Copy(messageIdSection, 0, Data, index, messageIdSection.Length);
            }
            catch
            {
                ErrorHandler.Error(ErrorHandler.ErrorType.MessageEncodingError);
            }
        }

        public override void DecodeMessage(byte[] data)
        {
            try
            {
                Data = data;
                MessageId = (ushort)((data[2] << 8) | data[1]);
            }
            catch
            {
                ErrorHandler.Error(ErrorHandler.ErrorType.MessageDecodingError);
            }
        }
    }
}
