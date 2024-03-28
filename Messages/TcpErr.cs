using IPK_2024_1.Inner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPK_2024_1.Messages
{
    internal class TcpErr : TcpMessage
    {
        public void EncodeMessage(string displayName, string messageContent)
        {
            Message = new string($"{ContentError} {displayName} {IsStr} {messageContent}\r\n");
        }

        public override void DecodeMessage(string mesString)
        {
            try
            {
                var words = mesString.Split([' ']);

                DisplayName = words[2];
                MessageContent = string.Empty;
                //for (var i = 4; i < words.Length - 1; i++)
                //    MessageContent += words[i] + " ";
                //MessageContent += words[words.Length - 1].Substring(0, words[words.Length - 1].Length - 2);
                for (var i = 4; i < words.Length - 1; i++)
                    MessageContent += words[i] + " ";
                MessageContent += words[words.Length - 1];
            }
            catch
            {
                ErrorHandler.Error(ErrorHandler.ErrorType.TcpDecodingError);
            }
        }
    }
}
