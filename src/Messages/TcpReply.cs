/******************************************************************************
 *                                  IPK-2024-1
 *                                  TcpReply.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains class for TcpReply message
 *                  Last change: 27.03.23
 *****************************************************************************/

namespace IPK_2024_1.Messages
{
    internal class TcpReply : TcpMessage
    {
        public void EncodeMessage(bool result, string messageContent)
        {
            var res = result ? "OK" : "NOK";
            Message = new string($"{ContentReply} {res} {IsStr} {messageContent}\r");
        }

        public override void DecodeMessage(string mesString)
        {
            var words = mesString.Split([' ']);

            Result = words[1] == "OK"; MessageContent = string.Empty;
            for (var i = 3; i < words.Length - 1; i++)
                MessageContent += words[i] + " ";
            MessageContent += words[words.Length - 1];
        }
    }
}
