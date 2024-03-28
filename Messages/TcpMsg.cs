using System.Text;

namespace IPK_2024_1.Messages
{
    internal class TcpMsg : TcpMessage
    {
        public void EncodeMessage(string displayName, string messageContent)
        {
            Message = new string($"{ContentMessage} {displayName} {IsStr} {messageContent}\r\n");
        }

        public override void DecodeMessage(string mesString)
        {
            var words = mesString.Split([' ']);

            DisplayName = words[2];
            MessageContent = string.Empty;
            //for (var i = 3; i < words.Length - 1; i++)
            //    MessageContent += words[i] + " ";
            //MessageContent += words[words.Length - 1].Substring(0, words[words.Length - 1].Length - 2);

            for (var i = 4; i < words.Length - 1; i++)
                MessageContent += words[i] + " ";
            MessageContent += words[words.Length - 1];
        }
    }
}
