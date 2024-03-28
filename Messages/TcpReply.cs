using System.Text;

namespace IPK_2024_1.Messages
{
    internal class TcpReply : TcpMessage
    {
        public void EncodeMessage(bool result, string messageContent)
        {
            var res = result ? "OK" : "NOK";
            Message = new string($"{ContentReply} {res} {IsStr} {messageContent}\r\n");
        }

        public override void DecodeMessage(string mesString)
        {
            var words = mesString.Split([' ']);

            Result = words[1] == "OK"; MessageContent = string.Empty;
            //for (var i = 3; i < words.Length - 1; i++)
            //    MessageContent += words[i] + " ";
            //MessageContent += words[words.Length - 1].Substring(0, words[words.Length - 1].Length - 2);
            for (var i = 3; i < words.Length - 1; i++)
                MessageContent += words[i] + " ";
            MessageContent += words[words.Length - 1];
        }
    }
}
