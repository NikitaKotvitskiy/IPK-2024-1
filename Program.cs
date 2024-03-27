using IPK_2024_1.Inner;

namespace IPK_2024_1
{
    internal abstract class Program
    {
        public static void Main(string?[] args)
        {
            Parameters.ProcessCla(args);

            switch (Parameters.Mode)
            {
                case Parameters.ProtocolType.Udp:
                    UdpClientLogic.Start();
                    break;
                case Parameters.ProtocolType.Tcp:
                    TcpClient.Start();
                    break;
                default:
                    ErrorHandler.Error(ErrorHandler.ErrorType.BadError);
                    break;
            }
        }
    }
}