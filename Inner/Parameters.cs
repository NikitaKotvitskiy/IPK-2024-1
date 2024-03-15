using System.Net;

namespace IPK_2024_1.Inner;

internal abstract class Parameters
{
    public enum ProtocolType
    {
        Udp,
        Tcp
    }

    public static ProtocolType? Mode;
    public static ushort Port = 4567;
    public static ushort Timeout = 250;
    public static short MaxRetransmissions = 3;
    public static bool Help;
    public static IPAddress? Ip;

    public static void ProcessCla(string?[] args)
    {
        var index = -1;
        while (true)
        {
            var arg = GetNext();
            switch (arg)
            {
                case "-t":
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
                case "-s":
                    arg = GetNext();
                    if (string.IsNullOrEmpty(arg))
                        ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
                    else if (!IPAddress.TryParse(arg, out Ip))
                        try
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
                case "-p":
                    arg = GetNext();
                    if (arg == null || !ushort.TryParse(arg, out Port))
                        ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
                    continue;
                case "-d":
                    arg = GetNext();
                    if (arg == null || !ushort.TryParse(arg, out Timeout))
                        ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
                    continue;
                case "r":
                    arg = GetNext();
                    if (arg == null || !short.TryParse(arg, out MaxRetransmissions))
                        ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
                    continue;
                case "-h":
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

        if (Mode == null || Ip == null)
            ErrorHandler.Error(ErrorHandler.ErrorType.ClaErr);
        return;

        string? GetNext() => ++index >= args.Length ? null : args[index];
    }

    public static void PrintParameters()
    {
        switch (Mode)
        {
            case ProtocolType.Tcp:
                Console.WriteLine("TCP");
                break;
            case ProtocolType.Udp:
                Console.WriteLine("UDP");
                break;
            default:
                ErrorHandler.Error(ErrorHandler.ErrorType.BadError);
                break;
        }
        Console.WriteLine(Ip);
        Console.WriteLine(Port);
        Console.WriteLine(Timeout);
        Console.WriteLine(Help);
    }
}