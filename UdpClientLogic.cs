using System.Net;
using System.Net.Sockets;
using IPK_2024_1.Inner;
using IPK_2024_1.Messages;

namespace IPK_2024_1;
    
internal abstract class UdpClientLogic
{
    private static ushort _lastMessageId;
    public static ushort GetNewId() => _lastMessageId++;
    private static readonly HashSet<ushort> ServerMessageIds = [];

    private static UdpClient _client = null!;

    private static readonly Semaphore ConfirmationSemaphore = new Semaphore(0, 1);
    private static readonly object ConfIdLock = new object();
    private static ushort? _confId;

    private static readonly Semaphore WaitForReplySemaphore = new Semaphore(0, 1);
    
    public static void Start()
    {
        try
        {
            Console.CancelKeyPress += delegate(object? sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                var bye = new UdpBye();
                bye.EncodeMessage();
                SendMessage(bye, false);
                _client.Close();
                Environment.Exit(0);
            };
            
            _client = new UdpClient(0, AddressFamily.InterNetwork);
            var receiveThread = new Thread(Receiver);
            receiveThread.Start();

            ConnectionFsm();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }
        finally
        {
            _client.Close();
        }
    }

    private static void ConnectionFsm()
    {
        while (true)
        {
            var exit = false;
            
            Command com;
            UdpMessage mes;

            switch (ClientFsm.CurrentState)
            {
                case ClientFsm.State.Auth:
                    com = CommandLine.GetCommand();
                    if (com.Type != Command.CommandType.Auth)
                    {
                        Console.WriteLine(ClientFsm.AuthHelpMessage);
                        continue;
                    }

                    mes = new UdpAuth();
                    ((UdpAuth)mes).EncodeMessage(com.Username, com.DisplayName, com.Secret);
                    if (!SendMessage(mes, true)) 
                        Terminate("No response");
                    WaitForReplySemaphore.WaitOne();
                    break;
                case ClientFsm.State.Open:
                    com = CommandLine.GetCommand();
                    if (com.Type == Command.CommandType.Join)
                    {
                        mes = new UdpJoin();
                        ((UdpJoin)mes).EncodeMessage(com.ChannelId, ClientFsm.DisplayName);
                        if (!SendMessage(mes, true))
                            Terminate("No response");
                        WaitForReplySemaphore.WaitOne();
                        break;
                    }
                    if (com.Type == Command.CommandType.Message)
                    {
                        mes = new UdpMsg();
                        ((UdpMsg)mes).EncodeMessage(ClientFsm.DisplayName, com.MessageContent);
                        if (!SendMessage(mes, true))
                            Terminate("No response");
                    }
                    break;
                case ClientFsm.State.Exit:
                    exit = true;
                    break;
            }
            
            if (exit)
                break;
        }
    }

    private static bool SendMessage(UdpMessage message, bool confirmationRequired)
    {
        byte[] data = message.Data!;
        var endPoint = new IPEndPoint(Parameters.Ip, Parameters.Port);
        if (!confirmationRequired)
        {
            _client.Send(data, data.Length, endPoint);
            return true;
        }

        lock (ConfIdLock)
            _confId = message.MessageId;
        var tries = 1 + Parameters.MaxRetransmissions;
        while (tries > 0)
        {
            _client.Send(data, data.Length, endPoint);
            var result = ConfirmationSemaphore.WaitOne(Parameters.Timeout);
            if (result)
                return true;
            tries--;
        }
        return false;
    }
    
    private static void Receiver()
    {
        try
        {
            IPEndPoint? remoteEndPoint = null;

            while (true)
            {   
                var receivedData = _client.Receive(ref remoteEndPoint);
                if (remoteEndPoint.Port != Parameters.Port)
                    lock (Parameters.PortLock) Parameters.Port = (ushort)remoteEndPoint.Port;
                
                ProcessMessageFromServer(receivedData);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    
    private static void ProcessMessageFromServer(byte[] messageData)
    {
        switch (messageData[0])
        {
            case 0x00:
                var confirmationMessage = new UdpConfirm();
                confirmationMessage.DecodeMessage(messageData);
                var refId = confirmationMessage.RefMessageId;
                lock (ConfIdLock) 
                    if (_confId == refId)
                    {
                        ConfirmationSemaphore.Release();
                        _confId = null;
                    }
                break;
            case 0x01:
                var replyMessage = new UdpReply();
                replyMessage.DecodeMessage(messageData);
                var confToReplyMessage = new UdpConfirm();
                confToReplyMessage.EncodeMessage(replyMessage.MessageId);
                SendMessage(confToReplyMessage, false);
                if (ServerMessageIds.Add(replyMessage.MessageId))
                {
                    Console.WriteLine(replyMessage.MessageContent);
                    if (ClientFsm.CurrentState == ClientFsm.State.Auth && replyMessage.Result == true)
                        ClientFsm.CurrentState = ClientFsm.State.Open;
                    WaitForReplySemaphore.Release();
                }
                break;
            case 0x04:
                var msgMessage = new UdpMsg();
                msgMessage.DecodeMessage(messageData);
                var confToMsgMessage = new UdpConfirm();
                confToMsgMessage.EncodeMessage(msgMessage.MessageId);
                SendMessage(confToMsgMessage, false);
                if (ServerMessageIds.Add(msgMessage.MessageId))
                    Console.WriteLine($"{msgMessage.DisplayName}: {msgMessage.MessageContent}");
                break;
            case 0xFE:
                var errMessage = new UdpErr();
                errMessage.DecodeMessage(messageData);
                var confToErrMessage = new UdpConfirm();
                SendMessage(confToErrMessage, false);
                if (ServerMessageIds.Add(errMessage.MessageId))
                {
                    Console.WriteLine($"{errMessage.DisplayName}: {errMessage.MessageContent}");
                    Terminate();
                }
                break;
            case 0xFF:
                Terminate();
                break;
            default:
                Terminate("Bad message format from server");
                break;
        }
    }

    private static void Terminate(string? reason = null)
    {
        var bye = new UdpBye();
        bye.EncodeMessage();
        SendMessage(bye, false);
        _client.Close();
        
        if (reason != null)
            Console.WriteLine(reason);
        
        Environment.Exit(0);
    }
}