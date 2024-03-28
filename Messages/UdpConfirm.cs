using IPK_2024_1.Inner;

namespace IPK_2024_1.Messages
{
    internal class UdpConfirm : UdpMessage
    {
        public void EncodeMessage(ushort refMessageId)
        {
            try
            {
                RefMessageId = refMessageId;

                var refMessageIdSection = BitConverter.GetBytes(refMessageId);
                Data = new byte[3];
                var index = 0;
                Data[index++] = 0x00;
                Array.Copy(refMessageIdSection, 0, Data, index, refMessageIdSection.Length);
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
                RefMessageId = (ushort)((data[2] << 8) | data[1]);
            }
            catch
            {
                ErrorHandler.Error(ErrorHandler.ErrorType.MessageDecodingError);
            }
        }
    }
}
