/******************************************************************************
 *                                  IPK-2024-1
 *                                CommandLine.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains logic for user input processing
 *                  Last change: 28.03.23
 *****************************************************************************/

namespace IPK_2024_1
{
    // This class represents a user command
    internal class Command
    {
        public enum CommandType // Enumeration with types of messages
        {
            Auth,
            Join,
            Message,
            None
        }

        public CommandType Type { get; private set; } = CommandType.None;   // Stores the current command type
        public string Username { get; private set; } = string.Empty;        // Stores the username value
        public string ChannelId { get; private set; } = string.Empty;       // Stores the channel ID value
        public string Secret { get; private set; } = string.Empty;          // Stores the secret
        public string DisplayName { get; private set; } = string.Empty;     // Strores the display name
        public string MessageContent { get; private set; } = string.Empty;  // Stores the message content

        public void SetCommandType(CommandType type) => Type = type;        // Sets new type if command

        public bool SetUsername(string username)                            // Checks the username format and sets it if correct
        {
            if (!CheckFields.CheckUserName(username))
                return false;
            Username = username;
            return true;
        }
        public bool SetChannelId(string channelId)                          // Checks the channel ID format and sets it if correct
        {
            if (!CheckFields.CheckChannelId(channelId))
                return false;
            ChannelId = channelId;
            return true;
        }
        public bool SetSecret(string secret)                                // Checks the secret format and sets it if correct
        {
            if (!CheckFields.CheckSecret(secret))
                return false;
            Secret = secret;
            return true;
        }
        public bool SetDisplayName(string displayName)                      // Checks the display name format and sets it if correct
        {
            if (!CheckFields.CheckDisplayName(displayName))
                return false;
            DisplayName = displayName;
            return true;
        }
        public bool SetMessageContent(string messageContent)                // Checks the message content format and sets it if correct
        {
            if (!CheckFields.CheckMessageContent(messageContent))
                return false;
            MessageContent = messageContent;
            return true;
        }
    }

    // This class contains logic for user command line
    internal abstract class CommandLine
    {
        private const string HelpMessage =  "You can use the following commands:\n" +
                                            "\t/auth {username} {secret} {displayName}\n" +
                                            "\t/join {channelId}\n" +
                                            "\t/rename {displayName}\n" +
                                            "\t/{msg}";
        private const string BadArguments = "Bad command arguments. Use /help to see the list of commands";
        private const string BadFormat = "Bad message format. Use /help to see the syntax of commands";

        // This method reads command from stdin, create new command, fill it with user input and return
        public static Command GetCommand()
        {
            Command newCommand = new();                     // Create new command object
            while (true)
            {
                var line = Console.ReadLine();              // Read user input
                var words = line?.Split([' ', '\t']);       // Split it
                if (words == null || words.Length == 0)     // Check the input format
                    return newCommand;


                switch (words[0])                           // Define the type based on the first word
                {
                    case "/auth":
                        if (words.Length != 4 || !newCommand.SetUsername(words[1]) || !newCommand.SetSecret(words[2]) ||
                            !newCommand.SetDisplayName(words[3]))
                        {
                            Console.WriteLine(BadArguments);
                            continue;
                        }
                        ClientFsm.SetDisplayName(newCommand.DisplayName);
                        newCommand.SetCommandType(Command.CommandType.Auth);
                        return newCommand;
                    case "/join":
                        if (words.Length != 2 || !newCommand.SetChannelId(words[1]))
                        {
                            Console.WriteLine(BadArguments);
                            continue;
                        }

                        newCommand.SetCommandType(Command.CommandType.Join);
                        return newCommand;
                    case "/rename":
                        if (words.Length != 2 || !newCommand.SetDisplayName(words[1]))
                        {
                            Console.WriteLine(BadArguments);
                            continue;
                        }
                        ClientFsm.SetDisplayName(newCommand.DisplayName);
                        continue;
                    case "/help":
                        Console.WriteLine(HelpMessage);
                        continue;
                    default:
                        if (line != null && line != string.Empty && line[0] != '/' && !newCommand.SetMessageContent(line))
                        {
                            Console.WriteLine(BadFormat);
                            continue;
                        }
                        newCommand.SetCommandType(Command.CommandType.Message);
                        return newCommand;
                }
            }
        }
    }
}
