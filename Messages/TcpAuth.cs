namespace IPK_2024_1.Messages
{
    internal class TcpAuth : TcpMessage
    {
        public void EncodeMessage(string username, string displayName, string secret)
        {
            Message = ($"{ContentAuth} {username} {AsStr} {displayName} {UsingStr} {secret}\r\n");
        }

        public override void DecodeMessage(string mesString)
        {
        }
    }
}
