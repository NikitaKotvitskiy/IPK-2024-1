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
                MessageId = UdpClient.GetNewId();

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
            try
            {
                Data = data;
                var startPosition = 1;

                MessageId = BitConverter.ToUInt16(data, startPosition);
                startPosition += 2;

                ChannelId = Encoding.ASCII.GetString(data, startPosition, DefineStringLength(data, ref startPosition));
                DisplayName =
                    Encoding.ASCII.GetString(data, startPosition, DefineStringLength(data, ref startPosition));
            }
            catch
            {
                ErrorHandler.Error(ErrorHandler.ErrorType.MessageDecodingError);
            }
        }
    }
}
