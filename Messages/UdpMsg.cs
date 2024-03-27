using IPK_2024_1.Inner;
using System.Text;

namespace IPK_2024_1.Messages
{
    internal class UdpMsg : UdpMessage
    {
        public void EncodeMessage(string displayName, string messageContent)
        {
            try
            {
                DisplayName = displayName;
                MessageContent = messageContent;
                MessageId = UdpClientLogic.GetNewId();

                var messageIdSection = BitConverter.GetBytes((ushort)MessageId);
                var displayNameSection = Encoding.ASCII.GetBytes(displayName);
                var messageContentSection = Encoding.ASCII.GetBytes(messageContent);

                Data = new byte[messageIdSection.Length + displayNameSection.Length + messageContentSection.Length + 3];

                var index = 0;

                Data[index++] = 0x04;
                Array.Copy(messageIdSection, 0, Data, index, messageIdSection.Length);
                index += messageIdSection.Length;
                Array.Copy(displayNameSection, 0, Data, index, displayNameSection.Length);
                index += displayNameSection.Length;
                Data[index++] = 0x00;
                Array.Copy(messageContentSection, 0, Data, index, messageContentSection.Length);
                index += messageContentSection.Length;
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

                MessageId = (ushort)((ushort)(data[startPosition + 1] << 8) | data[startPosition]);
                startPosition += 2;

                DisplayName =
                    Encoding.ASCII.GetString(data, startPosition, DefineStringLength(data, ref startPosition));
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
