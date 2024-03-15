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
    }
}
