using System.Text;
using IPK_2024_1.Inner;

namespace IPK_2024_1.Messages
{
    internal class UdpAuth : UdpMessage
    {
        public void EncodeMessage(string username, string displayName, string secret)
        {
            try
            {
                Username = username;
                DisplayName = displayName;
                Secret = secret;
                MessageId = UdpClient.GetNewId();

                var messageIdSection = BitConverter.GetBytes((ushort)MessageId);
                var usernameSection = Encoding.ASCII.GetBytes(username);
                var displayNameSection = Encoding.ASCII.GetBytes(displayName);
                var secretSection = Encoding.ASCII.GetBytes(secret);

                Data = new byte[messageIdSection.Length + usernameSection.Length + displayNameSection.Length +
                                secretSection.Length + 4];
                var index = 0;

                Data[index++] = 0x02;
                Array.Copy(messageIdSection, 0, Data, index, messageIdSection.Length);
                index += messageIdSection.Length;
                Array.Copy(usernameSection, 0, Data, index, usernameSection.Length);
                index += usernameSection.Length;
                Data[index++] = 0x00;
                Array.Copy(displayNameSection, 0, Data, index, displayNameSection.Length);
                index += displayNameSection.Length;
                Data[index++] = 0x00;
                Array.Copy(secretSection, 0, Data, index, secretSection.Length);
                index += secretSection.Length;
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

                Username = Encoding.ASCII.GetString(data, startPosition, DefineStringLength(data, ref startPosition));
                DisplayName = Encoding.ASCII.GetString(data, startPosition, DefineStringLength(data, ref startPosition));
                Secret = Encoding.ASCII.GetString(data, startPosition, DefineStringLength(data, ref startPosition));
            }
            catch
            {
                ErrorHandler.Error(ErrorHandler.ErrorType.MessageDecodingError);
            }
        }
    }
}
