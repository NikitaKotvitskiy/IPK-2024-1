using System.Text.RegularExpressions;

namespace IPK_2024_1.Messages
{
    internal abstract class TcpMessage
    {
        public const string IsStr = "IS";
        public const string AsStr = "AS";
        public const string UsingStr = "USING";

        public const string ContentJoin = "JOIN";
        public const string ContentAuth = "AUTH";
        public const string ContentMessage = "MSG FROM";
        public const string ContentError = "ERR FROM";
        public const string ContentReply = "REPLY";
        public const string ContentBye = "BYE";

        public const string IdPattern = @"^[\w-]{1,20}$";
        public const string SecretPattern = @"^[\w-]{1,128}$";
        public const string ContentPattern = @"^[\w\s-]{1,1400}$";
        public const string DNamePattern = @"^[\w\s-]{1,20}$";

        public string Message { get; protected set; } = null!;

        public abstract void DecodeMessage(string mesString);

        public string? DisplayName { get; protected set; }
        public string? MessageContent { get; protected set; }
        public string? Username { get; protected set; }
        public string? ChannelId { get; protected set; }
        public string? Secret { get; protected set; }
        public bool? Result { get; protected set;  }
    }
}
