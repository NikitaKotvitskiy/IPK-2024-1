/******************************************************************************
 *                                  IPK-2024-1
 *                                  TcpJoin.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains class for TcpJoin message
 *                  Last change: 27.03.23
 *****************************************************************************/

namespace IPK_2024_1.Messages
{
    internal class TcpJoin : TcpMessage
    {
        public void EncodeMessage(string channelId, string displayName)
        {
            Message = new string($"{ContentJoin} {channelId} {AsStr} {displayName}\r");
        }

        public override void DecodeMessage(string mesString)
        {
            // Server never sends join message
        }
    }
}
