/******************************************************************************
 *                                  IPK-2024-1
 *                              TcpClientLogic.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains logic for tdp 
 *                               variant of client application
 *                  Last change: 27.03.23
 *****************************************************************************/

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

    private static Semaphore _waiter = new Semaphore(0, 1); // Semaphore used for waiting for reply message

    public static void Start()
    {
        try
        {
            Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e) // Delegate for right exit from application
            {
                e.Cancel = true;
                var bye = new TcpBye();
                bye.EncodeMessage();
                SendMessage(bye.Message);
                _client.Close();
                Environment.Exit(0);
            };

            _client = new TcpClient(Parameters.Ip.ToString(), Parameters.Port); // Create new TcpClient, stream, reader and writer
            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.ASCII);
            _writer = new StreamWriter(_stream, Encoding.ASCII);

            var receiveThread = new Thread(Receiver);   // Create thread for receiver
            receiveThread.Start();

            ConnectionFsm();    // Start processing user input
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }
        finally
        {
            Console.WriteLine("Vse, idi nahui");
            _client.Close();
        }
    }

    // This method contains the loop for user input
    // Its behaviour is based om the current FSM state
    private static void ConnectionFsm()
    {
        while (true)
        {
            bool exit = false;

            Command com;
            TcpMessage mes;

            switch (ClientFsm.CurrentState)
            {
                case ClientFsm.State.Auth:                          // Auth case means /auth command is required
                    com = CommandLine.GetCommand();                 // Getting command from user
                    if (com.Type != Command.CommandType.Auth)       // Check the type of the command
                    {
                        Console.WriteLine(ClientFsm.AuthHelpMessage);
                        continue;
                    }
                    mes = new TcpAuth();                            // Create and encode TcpAuth message
                    ((TcpAuth)mes).EncodeMessage(com.Username, com.DisplayName, com.Secret);
                    SendMessage(mes.Message);                       // Send auth message
                    _waiter.WaitOne();                              // Wait for reply
                    break;
                case ClientFsm.State.Open:                          // Open case means msg, rename, join types of command are acceptable (but rename is processed inside CommandLine)
                    com = CommandLine.GetCommand();                 // Getting command from user
                    if (com.Type == Command.CommandType.Join)       // If join, form join message, send it, wait for reply
                    {
                        mes = new TcpJoin();
                        ((TcpJoin)mes).EncodeMessage(com.ChannelId, ClientFsm.DisplayName);
                        SendMessage(mes.Message);
                        _waiter.WaitOne();
                        break;
                    }
                    if (com.Type == Command.CommandType.Message)    // If msg, form msg message, send it
                    {
                        mes = new TcpMsg();
                        ((TcpMsg)mes).EncodeMessage(ClientFsm.DisplayName, com.MessageContent);
                        SendMessage(mes.Message);
                    }
                    break;
                case ClientFsm.State.Exit:                          // exit state means its time to terminate connection
                    exit = true;
                    break;
            }

            if (exit)
            {
                var bye = new TcpBye();
                ((TcpBye)bye).EncodeMessage();
                SendMessage(bye.Message);
                break;
            }
        }
    }

    // This method send a message to the server
    private static void SendMessage(string message)
    {
        _writer.WriteLine(message);
        _writer.Flush();
    }

    // This method receives messages from the server
    private static void Receiver()
    {
        while (true)
        {
            var message = _reader.ReadLine();
            if (message != null)
                ProcessMessageFromServer(message);
        }
    }

    // This method processes messages from the server
    private static void ProcessMessageFromServer(string message)
    {
        var words = message.Split(' ');     // Split the message by single words
        TcpMessage mes;
        switch (words[0])                                 // The first words indicate the type of message  
        {
            case "ERR":                                   // In err case: print err message and exit
                mes = new TcpErr();
                ((TcpErr)mes).DecodeMessage(message);
                Console.WriteLine($"{mes.DisplayName}: {mes.MessageContent}");
                ClientFsm.CurrentState = ClientFsm.State.Exit;
                break;
            case "REPLY":                                 // In reply case: print reply message and release the waiter semaphore
                mes = new TcpReply();
                ((TcpReply)mes).DecodeMessage(message);
                Console.WriteLine(mes.MessageContent);
                if (ClientFsm.CurrentState == ClientFsm.State.Auth && mes.Result == true)
                    ClientFsm.CurrentState = ClientFsm.State.Open;
                _waiter.Release();
                break;
            case "MSG":                                   // In msg case: print message
                mes = new TcpMsg();
                ((TcpMsg)mes).DecodeMessage(message);
                Console.WriteLine($"{mes.DisplayName}: {mes.MessageContent}");
                break;
            case "BYE":                                   // In bye case: exit
                ClientFsm.CurrentState = ClientFsm.State.Exit;
                break;
        }
    }
}