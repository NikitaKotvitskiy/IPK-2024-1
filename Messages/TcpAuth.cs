using IPK_2024_1.Inner;
using System.Text;

namespace IPK_2024_1.Messages
{
    internal class TcpAuth : TcpMessage
    {
        public void EncodeMessage(string username, string displayName, string secret)
        {
            Message = new string($"{ContentAuth} {username} {AsStr} {displayName} {UsingStr} {secret}\r\n");
        }

        public override void DecodeMessage(string mesString)
        {
        }
    }
}
