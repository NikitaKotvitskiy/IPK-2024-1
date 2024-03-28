/******************************************************************************
 *                                  IPK-2024-1
 *                                ErrorHandler.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: Simple error handler with termination 
 *                               functionality
 *                  Last change: 27.03.23
 *****************************************************************************/

namespace IPK_2024_1.Inner
{
    internal static class ErrorHandler
    {
        public enum ErrorType
        {
            BadError = -1,
            ClaErr = 1,
            BadServer = 2,
            SocketError = 3,
            MessageDecodingError = 6,
            MessageEncodingError = 7,
            ConnectionIsLost = 8,
            ConfirmationSendError = 9,
            MessageSendError = 10,
            MessageReceiveError = 11,
            TcpDecodingError = 12
        }

        public static void Error(ErrorType type)
        {
            switch (type)
            {
                case ErrorType.ClaErr:
                    Console.Error.WriteLine("Error: invalid command line arguments");
                    break;
                case ErrorType.BadServer:
                    Console.Error.WriteLine("Error: invalid server value");
                    break;
                case ErrorType.SocketError:
                    Console.Error.WriteLine("Error: failed to create socket");
                    break;
                case ErrorType.MessageDecodingError:
                    Console.Error.WriteLine("Error: failed to decode message from server");
                    break;
                case ErrorType.MessageEncodingError:
                    Console.Error.WriteLine("Error: failed to encode message");
                    break;
                case ErrorType.ConnectionIsLost:
                    Console.Error.WriteLine("Error: connection with server is lost");
                    break;
                case ErrorType.ConfirmationSendError:
                    Console.Error.WriteLine("Error: failed to send confrmation message");
                    break;
                case ErrorType.MessageSendError:
                    Console.Error.WriteLine("Error: failed to send message");
                    break;
                case ErrorType.MessageReceiveError:
                    Console.Error.WriteLine("Error: faild to receive message");
                    break;
                case ErrorType.TcpDecodingError:
                    Console.Error.WriteLine("Error: failed to decode TCP message");
                    break;
                case ErrorType.BadError:
                default:
                    Console.Error.WriteLine("Error: bad error number!");
                    break;
            }

            Environment.Exit((int)type);
        }
    }
}
