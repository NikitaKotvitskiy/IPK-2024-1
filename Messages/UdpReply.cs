using System.Text;
using IPK_2024_1.Inner;

namespace IPK_2024_1.Messages
{
    internal class UdpReply : UdpMessage
    {
        public void EncodeMessage(bool result, ushort refMessageId, string messageContent)
        {
            try
            {
                Result = result;
                RefMessageId = refMessageId;
                MessageContent = messageContent;
                MessageId = UdpClientLogic.GetNewId();

                var messageIdSection = BitConverter.GetBytes((ushort)MessageId);
                var refMessageIdSection = BitConverter.GetBytes(refMessageId);
                var messageContentsSection = Encoding.ASCII.GetBytes(messageContent);

                Data = new byte[messageIdSection.Length + refMessageIdSection.Length + messageContentsSection.Length +
                                3];
                var index = 0;

                Data[index++] = 0x01;
                Array.Copy(messageIdSection, 0, Data, index, messageIdSection.Length);
                index += messageIdSection.Length;
                Data[index++] = result ? (byte)0x01 : (byte)0x00;
                Array.Copy(refMessageIdSection, 0, Data, index, refMessageIdSection.Length);
                index += refMessageIdSection.Length;
                Array.Copy(messageContentsSection, 0, Data, index, messageContentsSection.Length);
                index += messageContentsSection.Length;
                Data[index] = 0x00;

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
