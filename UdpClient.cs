using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using IPK_2024_1.Inner;
using IPK_2024_1.Messages;

namespace IPK_2024_1;

internal class UdpClient
{
    private static ushort _lastMessageId = 0;
    public static ushort GetNewId() => _lastMessageId++;

    private static Socket? _socket;

    private static Semaphore _commandSemaphore = new(1, 1);
    private static Semaphore _senderSemapore = new(0, 1);
    private static Semaphore _receiverSemaphore = new(0, 1);
    private static Semaphore _confirmationSemaphore = new(0, 1);

    public static object _confirmationIdLocker = new();
    private static ushort? _messageIdToBeConfirmed;

    public static void Start()
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }
        catch
        {
            ErrorHandler.Error(ErrorHandler.ErrorType.SocketError);
        }

        var messageFactory = new Thread(MessageFactory);
        var receiveThread = new Thread(MessageReceiver);

        ClientFsm.CurrentState = ClientFsm.State.Auth;
        messageFactory.Start();
        receiveThread.Start();
    }

    public static void MessageFactory()
    {
        while(true)
        {
            _commandSemaphore.WaitOne();
            Command com = CommandLine.GetCommand();
            if (com.Type == Command.CommandType.None || !ClientFsm.CheckCommandValidity(com))
                continue;

            UdpMessage message;
            switch (com.Type)
            {
                case Command.CommandType.Auth:
                    message = new UdpAuth();
                    ((UdpAuth)message).EncodeMessage(com.Username, com.DisplayName, com.Secret);
                    MessageSender(message);
                    break;
                case Command.CommandType.Message:
                    message = new UdpMsg();
                    ((UdpMsg)message).EncodeMessage(com.DisplayName, com.MessageContent);
                    MessageSender(message);
                    break;
                case Command.CommandType.Join:
                    message = new UdpJoin();
                    ((UdpJoin)message).EncodeMessage(com.ChannelId, com.DisplayName);
                    MessageSender(message);
                    break;
                default:
                    ErrorHandler.Error(ErrorHandler.ErrorType.BadError);
                    break;
            }
        }
    }

    private static void SendConfirmMessage(UdpConfirm message)
    {
        try
        {
            var data = message.Data;
            if (data != null && _socket != null)
                _socket.BeginSend(data, 0, data.Length, SocketFlags.None, ConfirmationCallback, null);
            else
                throw new Exception();
        }
        catch
        {
            ErrorHandler.Error(ErrorHandler.ErrorType.ConfirmationSendError);
        }
    }
    private static void ConfirmationCallback(IAsyncResult ar)
    {
        try
        {
            if (_socket != null)
                _socket.EndSend(ar);
            else
                throw new Exception();
        }
        catch
        {
            ErrorHandler.Error(ErrorHandler.ErrorType.ConfirmationSendError);
        }
    }

    private static void MessageSender(UdpMessage message)
    {
        try
        {
            lock (_confirmationIdLocker) _messageIdToBeConfirmed = message.MessageId;
            var tries = Parameters.MaxRetransmissions + 1;
            var data = message.Data;
            if (Parameters.Ip == null)
                throw new Exception();
            var endPoint = new IPEndPoint(Parameters.Ip, Parameters.Port);

            while (tries > 0)
            {
                if (_socket != null && data != null)
                    _socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endPoint, SendCallback, null);
                else
                    throw new Exception();
                _senderSemapore.WaitOne();

                var signal = _confirmationSemaphore.WaitOne(Parameters.Timeout);

                if (signal)
                {
                    _commandSemaphore.Release();
                    return;
                }
                tries--;
            }
        }
        catch
        {
            ErrorHandler.Error(ErrorHandler.ErrorType.MessageSendError);
        }
    }
    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            if (_socket != null)
                _socket.EndSend(ar);
            else
                throw new Exception();

            _senderSemapore.Release();
        }
        catch
        {
            ErrorHandler.Error(ErrorHandler.ErrorType.MessageSendError);
        }
    }

    private static void MessageReceiver()
    {
        try
        {
            while (true)
            {
                var buffer = new byte[1024];
                if (_socket != null)
                    _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, buffer);
                else
                    throw new Exception();
            }
        }
        catch
        {
            ErrorHandler.Error(ErrorHandler.ErrorType.MessageReceiveError);
        }
    }
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            if (_socket == null)
                throw new Exception();

            var bytesRead = _socket.EndReceive(ar);

            IPEndPoint? remoteEndPoint = _socket.RemoteEndPoint as IPEndPoint;
            if (remoteEndPoint == null)
                throw new Exception();
            Parameters.Port = (ushort)remoteEndPoint.Port;

            byte[]? buffer = ar.AsyncState as byte[];
            if (buffer == null)
                throw new Exception();
            _receiverSemaphore.Release();

            ProcessMessage(buffer);
        }
        catch
        {
            ErrorHandler.Error(ErrorHandler.ErrorType.MessageReceiveError);
        }
    }

    private static void ProcessMessage(byte[] message)
    {

    }
}