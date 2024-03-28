using IPK_2024_1.Inner;
using IPK_2024_1.Messages;
using System.Net.Sockets;
using System.Text;

namespace IPK_2024_1;

internal abstract class TcpClientLogic
{
    private static TcpClient _client = null!;
    private static NetworkStream _stream = null!;
    private static StreamWriter _writer = null!;
    private static StreamReader _reader = null!;

    private static Semaphore _waiter = new Semaphore(0, 1);

    public static void Start()
    {
        try
        {
            Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                var bye = new TcpBye();
                bye.EncodeMessage();
                SendMessage(bye.Message);
                _client.Close();
                Environment.Exit(0);
            };

            _client = new TcpClient(Parameters.Ip.ToString(), Parameters.Port);
            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.ASCII);
            _writer = new StreamWriter(_stream, Encoding.ASCII);

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
            bool exit = false;

            Command com;
            TcpMessage mes;

            switch (ClientFsm.CurrentState)
            {
                case ClientFsm.State.Auth:
                    com = CommandLine.GetCommand();
                    if (com.Type != Command.CommandType.Auth)
                    {
                        Console.WriteLine(ClientFsm.AuthHelpMessage);
                        continue;
                    }
                    mes = new TcpAuth();
                    ((TcpAuth)mes).EncodeMessage(com.Username, com.DisplayName, com.Secret);
                    SendMessage(mes.Message);
                    _waiter.WaitOne();
                    break;
                case ClientFsm.State.Open:
                    com = CommandLine.GetCommand();
                    if (com.Type == Command.CommandType.Join)
                    {
                        mes = new TcpJoin();
                        ((TcpJoin)mes).EncodeMessage(com.ChannelId, ClientFsm.DisplayName);
                        SendMessage(mes.Message);
                        _waiter.WaitOne();
                        break;
                    }
                    if (com.Type == Command.CommandType.Message)
                    {
                        mes = new TcpMsg();
                        ((TcpMsg)mes).EncodeMessage(ClientFsm.DisplayName, com.MessageContent);
                        SendMessage(mes.Message);
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

    private static void SendMessage(string message)
    {
        _writer.WriteLine(message);
        _writer.Flush();
    }

    public static void Receiver()
    {
        while (true)
        {
            var message = _reader.ReadLine();
            if (message != null)
                ProcessMessageFromServer(message);
        }
    }

    private static void ProcessMessageFromServer(string message)
    {
        var words = message.Split(' ');
        TcpMessage mes;
        switch (words[0])
        {
            case "ERR":
                mes = new TcpErr();
                ((TcpErr)mes).DecodeMessage(message);
                Console.WriteLine($"{mes.DisplayName}: {mes.MessageContent}");
                ClientFsm.CurrentState = ClientFsm.State.Exit;
                break;
            case "REPLY":
                mes = new TcpReply();
                ((TcpReply)mes).DecodeMessage(message);
                Console.WriteLine(mes.MessageContent);
                if (ClientFsm.CurrentState == ClientFsm.State.Auth && mes.Result == true)
                    ClientFsm.CurrentState = ClientFsm.State.Open;
                _waiter.Release();
                break;
            case "MSG":
                mes = new TcpMsg();
                ((TcpMsg)mes).DecodeMessage(message);
                Console.WriteLine($"{mes.DisplayName}: {mes.MessageContent}");
                break;
            case "BYE":
                mes = new TcpBye();
                ((TcpBye)mes).EncodeMessage();
                SendMessage(mes.Message);
                ClientFsm.CurrentState = ClientFsm.State.Exit;
                break;
        }
    }
}