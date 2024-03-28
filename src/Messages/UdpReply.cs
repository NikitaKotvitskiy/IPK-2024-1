/******************************************************************************
 *                                  IPK-2024-1
 *                                  UdpReply.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains class for UdpReply message
 *                  Last change: 27.03.23
 *****************************************************************************/

using System.Text;
using IPK_2024_1.Inner;

namespace IPK_2024_1.Messages
{
    internal class UdpReply : UdpMessage
    {
        // There is no need do decode reply message, because client never sends it

        public override void DecodeMessage(byte[] data)
        {
            try
            {
                Data = data;
                var startPosition = 1;

                MessageId = (ushort)((data[startPosition + 1] << 8) | data[startPosition]);
                startPosition += 2;
                Result = data[startPosition++] != 0;
                RefMessageId = (ushort)((data[startPosition + 1] << 8) | data[startPosition]);
                startPosition += 2;
                MessageContent =
                    Encoding.ASCII.GetString(data, startPosition, DefineStringLength(data, ref startPosition));
            }
            catch
            {
                ErrorHandler.Error(ErrorHandler.ErrorType.MessageDecodingError);
            }
        }
    }
}
