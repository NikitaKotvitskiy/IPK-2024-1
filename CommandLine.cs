namespace IPK_2024_1
{
    internal class Command
    {
        public enum CommandType
        {
            Auth,
            Join,
            Rename,
            Help,
            Message,
            None
        }

        public CommandType Type { get; private set; } = CommandType.None;
        public string Username { get; private set; } = string.Empty;
        public string ChannelId { get; private set; } = string.Empty;
        public string Secret { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public string MessageContent { get; private set; } = string.Empty;

        public void SetCommandType(CommandType type) => Type = type;

        public bool SetUsername(string username)
        {
            if (!CheckFields.CheckUserName(username))
                return false;
            Username = username;
            return true;
        }
        public bool SetChannelId(string channelId)
        {
            if (!CheckFields.CheckChannelId(channelId))
                return false;
            ChannelId = channelId;
            return true;
        }
        public bool SetSecret(string secret)
        {
            if (!CheckFields.CheckSecret(secret))
                return false;
            Secret = secret;
            return true;
        }
        public bool SetDisplayName(string displayName)
        {
            if (!CheckFields.CheckDisplayName(displayName))
                return false;
            DisplayName = displayName;
            return true;
        }
        public bool SetMessageContent(string messageContent)
        {
            if (!CheckFields.CheckMessageContent(messageContent))
                return false;
            MessageContent = messageContent;
            return true;
        }

        public void DebugPrintCommand()
        {
            Console.WriteLine(Type);
            Console.WriteLine(Username);
            Console.WriteLine(ChannelId);
            Console.WriteLine(Secret);
            Console.WriteLine(DisplayName);
            Console.WriteLine(MessageContent);
        }
    }

    internal abstract class CommandLine
    {
        private const string BadArguments = "Bad command arguments. Use /help to see the list of commands";
        private const string BadFormat = "Bad message format. Use /help to see the syntax of commands";

        public static Command GetCommand()
        {
            Command newCommand = new();

            var line = Console.ReadLine();
            var words = line?.Split([' ', '\t']);
            if (words == null || words.Length == 0)
                return newCommand;

            switch (words[0])
            {
                case "/auth":
                    if (words.Length != 4 || !newCommand.SetUsername(words[1]) || !newCommand.SetSecret(words[2]) ||
                        !newCommand.SetDisplayName(words[3]))
                    {
                        Console.WriteLine(BadArguments);
                        return newCommand;
                    }
                    newCommand.SetCommandType(Command.CommandType.Auth);
                    return newCommand;
                case "/join":
                    if (words.Length != 2 || !newCommand.SetChannelId(words[1]))
                    {
                        Console.WriteLine(BadArguments);
                        return newCommand;
                    }
                    newCommand.SetCommandType(Command.CommandType.Join);
                    return newCommand;
                case "/rename":
                    if (words.Length != 2 || !newCommand.SetDisplayName(words[1]))
                    {
                        Console.WriteLine(BadArguments);
                        return newCommand;
                    }
                    newCommand.SetCommandType(Command.CommandType.Rename);
                    return newCommand;
                case "/help":
                    newCommand.SetCommandType(Command.CommandType.Help);
                    return newCommand;
                default:
                    if (line != null && line[0] != '/' && !newCommand.SetMessageContent(line))
                    {
                        Console.WriteLine(BadFormat);
                        return newCommand;
                    }
                    newCommand.SetCommandType(Command.CommandType.Message);
                    return newCommand;
            }
        }
    }
}
