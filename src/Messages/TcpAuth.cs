/******************************************************************************
 *                                  IPK-2024-1
 *                                  TcpAuth.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains class for TcpAuth message
 *                  Last change: 27.03.23
 *****************************************************************************/

namespace IPK_2024_1.Messages
{
    internal class TcpAuth : TcpMessage
    {
        public void EncodeMessage(string username, string displayName, string secret)
        {
            Message = ($"{ContentAuth} {username} {AsStr} {displayName} {UsingStr} {secret}\r");
        }

        public override void DecodeMessage(string mesString)
        {
            // Server never sends auth message
        }
    }
}
