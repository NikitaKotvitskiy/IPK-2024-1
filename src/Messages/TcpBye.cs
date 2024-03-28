/******************************************************************************
 *                                  IPK-2024-1
 *                                  TcpBye.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains class for TcpBye message
 *                  Last change: 27.03.23
 *****************************************************************************/

namespace IPK_2024_1.Messages
{
    internal class TcpBye : TcpMessage
    {
        public void EncodeMessage()
        {
            Message = new string($"{ContentBye}\r\n");
        }

        public override void DecodeMessage(string mesString)
        {
            // There is not anything to decode here!
        }
    }
}
