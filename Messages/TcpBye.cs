namespace IPK_2024_1.Messages
{
    internal class TcpBye : TcpMessage
    {
        public void EncodeMessage()
        {
            Message = new string($"{ContentBye}\r\n");
        }

        public override void DecodeMessage(string mesString)
        {
        }
    }
}
