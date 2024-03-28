/******************************************************************************
 *                                  IPK-2024-1
 *                                Parameters.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains CLA processing and stores
 *                               the connection parameters
 *                  Last change: 27.03.23
 *****************************************************************************/

using System.Net;

namespace IPK_2024_1.Inner;

internal abstract class Parameters
{
    public enum ProtocolType                        // Enumeration for two possible types of thr protocole
    {
        Udp,
        Tcp
    }

    public static ProtocolType? Mode;               // Chosen protocole
    public static object PortLock = new object();   // Port lock
    public static ushort Port = 4567;               // Current port
    public static ushort Timeout = 250;             // Confirmation timeout
    public static short MaxRetransmissions = 3;     // Count of retransmissions
    public static bool Help;                        // Print help
    public static IPAddress Ip = null!;             // Stores the ip

    public const string HelpMessage = "The following set of arguments can be used:\n" +
                                        "\t-t {udp|tcp} - sets the type of communication protocole (required)\n" +
                                        "\t-s {ip|host} - sets the server (required)\n" +
                                        "\t-p {port} - sets the initical remote port (4567 by default)\n" +
                                        "\t-r {count} - sets the max number of retransmissions (3 by default)\n" +
                                        "\t-d {time} - sets the timeout value in ms (250 by default)\n" +
                                        "\t-h - prints this help message";

    // This method processes CLA
    public static void ProcessCla(string?[] args)
    {
        var index = -1;
        while (true)
        {
            var arg = GetNext();
            switch (arg)
            {
                case "-t":  // Process type argument
                    arg = GetNext();
                    switch (arg)
                    {
                        case "udp":
                            Mode = ProtocolType.Udp;
                            break;
                        case "tcp":
                            Mode = ProtocolType.Tcp;
                            break;
                        default:
                            ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
                            break;
                    }
                    continue;
                case "-s":  // Process server argument
                    arg = GetNext();
                    if (string.IsNullOrEmpty(arg))
                        ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
                    else if (!IPAddress.TryParse(arg, out Ip!)) // Try to parse the server argument as IPAddress
                        try                                     // If failed, try to parse as domen name
                        {
                            var addresses = Dns.GetHostAddresses(arg);
                            if (addresses[0].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                Ip = addresses[0];
                            else throw new Exception();
                        }
                        catch
                        {
                            ErrorHandler.Error(ErrorHandler.ErrorType.BadServer);
                        }
                    continue;
                case "-p":  // Process port argument
                    arg = GetNext();
                    if (arg == null || !ushort.TryParse(arg, out Port))
                        ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
                    continue;
                case "-d":  // Process timeout argument
                    arg = GetNext();
                    if (arg == null || !ushort.TryParse(arg, out Timeout))
                        ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
                    continue;
                case "-r":   // Process count of retransmissions argument
                    arg = GetNext();
                    if (arg == null || !short.TryParse(arg, out MaxRetransmissions))
                        ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
                    continue;
                case "-h":  // Process help argument
                    Help = true;
                    continue;
                case null:
                    break;
                default:
                    ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
                    break;
            }
            break;
        }

        if ((Mode == null || Ip == null) && !Help)     // If one of required parameters wasn't set, write error message and exit
            ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
        return;

        string? GetNext() => ++index >= args.Length ? null : args[index];
    }
}