namespace IPK_2024_1
{
    internal abstract class ClientFsm
    {
        public enum State
        {
            Start,
            Auth,
            Open,
            Error,
            End
        }

        public static State CurrentState { get; set; } = State.Start;
        public static string DisplayName { get; private set; } = string.Empty;
        public static string SetDisplayName(string name) => DisplayName = name;

        private const string _authHelpMessage = "For authorization type:\n" +
                                                "\t\\auth {username} {password} {display name}";
        private const string _openHelpMessage = "You can use the following set of commands:\n" +
                                                "\t\\join {channel ID} - to join channel with specified ID\n" +
                                                "\t\\rename {display name} - to change your display name";

        public static bool CheckCommandValidity(Command command)
        {
            switch (command.Type)
            {
                case Command.CommandType.Auth:
                    return CurrentState == State.Auth;
                case Command.CommandType.Join:
                case Command.CommandType.Message:
                    return CurrentState == State.Open;
                case Command.CommandType.Rename:
                    SetDisplayName(command.DisplayName);
                    return false;
                case Command.CommandType.Help:
                default:
                    switch (CurrentState)
                    {
                        case State.Start:
                        case State.Auth:
                            Console.WriteLine(_authHelpMessage);
                            break;
                        case State.Open:
                            Console.WriteLine(_openHelpMessage);
                            break;
                        default:
                            break;
                    }
                    return false;
            }
        }
    }
}
