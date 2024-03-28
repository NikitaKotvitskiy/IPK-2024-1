/******************************************************************************
 *                                  IPK-2024-1
 *                                  Program.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains an entry point, CLA 
 *                               processing and starting connection
 *                  Last change: 27.03.23
 *****************************************************************************/

using IPK_2024_1.Inner;

namespace IPK_2024_1
{
    internal abstract class Program
    {
        public static void Main(string?[] args)
        {
            Parameters.ProcessCla(args);            // Process CLA
            if (Parameters.Help)                    // If help is requested, print the help message and return
            {
                Console.WriteLine(Parameters.HelpMessage);
                return;
            }

            switch (Parameters.Mode)                // Start the right type of connection
            {
                case Parameters.ProtocolType.Udp:
                    UdpClientLogic.Start();
                    break;
                case Parameters.ProtocolType.Tcp:
                    TcpClientLogic.Start();
                    break;
                default:
                    ErrorHandler.Error(ErrorHandler.ErrorType.BadError);
                    break;
            }
        }
    }
}