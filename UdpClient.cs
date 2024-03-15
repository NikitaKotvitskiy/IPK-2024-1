using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using IPK_2024_1.Inner;
using IPK_2024_1.Messages;

namespace IPK_2024_1;

internal class UdpClient
{
    private static ushort _lastMessageId = 0;
    public static ushort GetNewId() => _lastMessageId++;

    public static void Start()
    {
        Console.WriteLine("UDP implementation is in progress...");
    }
}