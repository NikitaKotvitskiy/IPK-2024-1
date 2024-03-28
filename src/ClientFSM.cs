/******************************************************************************
 *                                  IPK-2024-1
 *                              UdpClientLogic.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains FSM data
 *                        Last change: 28.03.23
 *****************************************************************************/

namespace IPK_2024_1
{
    internal abstract class ClientFsm
    {
        public enum State       // Enumeration with all possible states
        {
            Auth,
            WaitForAuthReply,
            Open,
            WaitForJoinReply,
            Exit,
            End
        }

        public static object FsmStateLock = new object();               // Lock for client FSM state
        public static State CurrentState { get; set; } = State.Auth;    // Stores the current state
        
        public static string DisplayName { get; private set; } = string.Empty;  // Stores the current display name
        public static string SetDisplayName(string name) => DisplayName = name; // Sets the new display name

        public const string AuthHelpMessage = "For authorization type:\n" +
                                                "\t/auth {username} {password} {display name}";
    }
}
