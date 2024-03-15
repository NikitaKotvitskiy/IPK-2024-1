using IPK_2024_1.Inner;

namespace IPK_2024_1.Messages
{
    internal class UdpBye : UdpMessage
    {
        public void EncodeMessage()
        {
            try
            {
                MessageId = UdpClient.GetNewId();
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
                MessageId = BitConverter.ToUInt16(data, 1);
            }
            catch
            {
                ErrorHandler.Error(ErrorHandler.ErrorType.MessageDecodingError);
            }
        }
    }
}
