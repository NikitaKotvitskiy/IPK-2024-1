/******************************************************************************
 *                                  IPK-2024-1
 *                                  TcpMsg.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains class for TcpMsg message
 *                  Last change: 27.03.23
 *****************************************************************************/

namespace IPK_2024_1.Messages
{
    internal class TcpMsg : TcpMessage
    {
        public void EncodeMessage(string displayName, string messageContent)
        {
            Message = new string($"{ContentMessage} {displayName} {IsStr} {messageContent}\r");
        }

        public override void DecodeMessage(string mesString)
        {
            var words = mesString.Split([' ']);

            DisplayName = words[2];
            MessageContent = string.Empty;

            for (var i = 4; i < words.Length - 1; i++)
                MessageContent += words[i] + " ";
            MessageContent += words[words.Length - 1];
        }
    }
}
