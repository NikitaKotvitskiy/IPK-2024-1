using System.Text;

namespace IPK_2024_1.Messages
{
    internal class TcpJoin : TcpMessage
    {
        public void EncodeMessage(string channelId, string displayName)
        {
            Message = new string($"{ContentJoin} {channelId} {AsStr} {displayName}\r\n");
        }

        public override void DecodeMessage(string mesString)
        {
        }
    }
}
