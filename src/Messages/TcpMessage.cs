/******************************************************************************
 *                                  IPK-2024-1
 *                                TcpMessage.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains an abstract class for 
 *                               TcpMessages
 *                  Last change: 27.03.23
 *****************************************************************************/

namespace IPK_2024_1.Messages
{
    internal abstract class TcpMessage
    {
        // Constant strings for message forming
        protected const string IsStr = "IS";
        protected const string AsStr = "AS";
        protected const string UsingStr = "USING";

        protected const string ContentJoin = "JOIN";
        protected const string ContentAuth = "AUTH";
        protected const string ContentMessage = "MSG FROM";
        protected const string ContentError = "ERR FROM";
        protected const string ContentReply = "REPLY";
        protected const string ContentBye = "BYE";

        public string Message { get; protected set; } = null!;  // Contains message string

        public abstract void DecodeMessage(string mesString);   // Decodes messages from their strings

        // Message parameters
        public string? DisplayName { get; protected set; }
        public string? MessageContent { get; protected set; }
        public bool? Result { get; protected set;  }
    }
}
