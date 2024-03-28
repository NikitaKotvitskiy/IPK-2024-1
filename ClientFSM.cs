namespace IPK_2024_1
{
    internal abstract class ClientFsm
    {
        public enum State
        {
            Auth,
            WaitForAuthReply,
            Open,
            WaitForJoinReply,
            Exit,
            End
        }

        public static object FsmStateLock = new object();
        public static State CurrentState { get; set; } = State.Auth;
        
        public static string DisplayName { get; private set; } = string.Empty;
        public static string SetDisplayName(string name) => DisplayName = name;

        public const string AuthHelpMessage = "For authorization type:\n" +
                                                "\t/auth {username} {password} {display name}";

        public static bool CheckCommandValidity(Command command)
        {
            return true;
        }
    }
}
