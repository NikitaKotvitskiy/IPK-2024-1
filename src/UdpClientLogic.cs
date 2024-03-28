/******************************************************************************
 *                                  IPK-2024-1
 *                              UdpClientLogic.cs
 * 
 *                  Authors: Nikita Kotvitskiy (xkotvi01)
 *                  Description: This file contains logic for udp 
 *                               variant of client application
 *                  Last change: 28.03.23
 *****************************************************************************/

using System.Net;
using System.Net.Sockets;
using IPK_2024_1.Inner;
using IPK_2024_1.Messages;

namespace IPK_2024_1;
    
internal abstract class UdpClientLogic
{
    private static ushort _lastMessageId;                           // Output message ID counter
    public static ushort GetNewId() => _lastMessageId++;            // New output message id generator
    private static readonly HashSet<ushort> ServerMessageIds = [];  // Map structure for input messages ids

    private static UdpClient _client = null!;                       // UdpClient used for communication

    private static readonly Semaphore ConfirmationSemaphore = new Semaphore(0, 1);  // Semaphore used for waiting for confirmation message
    private static readonly object ConfIdLock = new object();                       // Lock used for access to _confId variable  
    private static ushort? _confId;                                                 // Stores the id of the last sent message

    private static readonly Semaphore WaitForReplySemaphore = new Semaphore(0, 1);  // Semaphore used for waiting for reply
    
    public static void Start()
    {
        try
        {
            Console.CancelKeyPress += delegate(object? sender, ConsoleCancelEventArgs e) // Delegate for right exit from application
            {
                e.Cancel = true;
                var bye = new UdpBye();
                bye.EncodeMessage();
                SendMessage(bye, false);
                _client.Close();
                Environment.Exit(0);
            };
            
            _client = new UdpClient(0, AddressFamily.InterNetwork); // Creation of new UdpClient

            var receiveThread = new Thread(Receiver); // Creation of the receiver thread
            receiveThread.Start();

            ConnectionFsm(); // Start the user input processor
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

    // This method contains the loop for user input
    // Its behaviour is based om the current FSM state
    private static void ConnectionFsm()
    {
        while (true)
        {
            var exit = false;
            
            Command com;
            UdpMessage mes;

            switch (ClientFsm.CurrentState)
            {               
                case ClientFsm.State.Auth:                      // Auth state means that /auth message is required
                    com = CommandLine.GetCommand();             // Get new command from command line
                    if (com.Type != Command.CommandType.Auth)   // If this is not an auth command, write about it and start again
                    {
                        Console.WriteLine(ClientFsm.AuthHelpMessage);
                        continue;
                    }

                    mes = new UdpAuth();                        // Create new auth message and fill it with data from auth command
                    ((UdpAuth)mes).EncodeMessage(com.Username, com.DisplayName, com.Secret);
                    if (!SendMessage(mes, true))                // Send message to the server, terminate if didn't get response 
                        Terminate("No response");
                    WaitForReplySemaphore.WaitOne();            // Wait for reply message
                    break;
                case ClientFsm.State.Open:                      // Open state means thar user can send messages, join channels and rename himself
                    com = CommandLine.GetCommand();             // Get new command from command line
                    if (com.Type == Command.CommandType.Join)   // If join command was got, encode, send, and wait for reply
                    {
                        mes = new UdpJoin();
                        ((UdpJoin)mes).EncodeMessage(com.ChannelId, ClientFsm.DisplayName);
                        if (!SendMessage(mes, true))
                            Terminate("No response");
                        WaitForReplySemaphore.WaitOne();
                        break;
                    }
                    if (com.Type == Command.CommandType.Message)    // If message command was got, encode and send
                    {
                        mes = new UdpMsg();
                        ((UdpMsg)mes).EncodeMessage(ClientFsm.DisplayName, com.MessageContent);
                        if (!SendMessage(mes, true))
                            Terminate("No response");
                    }
                    break;
                case ClientFsm.State.Exit: // Exit state means breake the loop and finish
                    exit = true;
                    break;
            }
            
            if (exit)
                break;
        }
    }

    // This method sends message to the server
    // UdpMessage message parameter contains message to send
    // bool confirmationRequired says if confirmation message should be received after sending
    private static bool SendMessage(UdpMessage message, bool confirmationRequired)
    {
        byte[] data = message.Data!;                                        // Get data from message
        var endPoint = new IPEndPoint(Parameters.Ip, Parameters.Port);      // Create endPoint with actual port
        if (!confirmationRequired)                                          // If confirmation IS NOT required, just send the message and return true (successful)
        {
            _client.Send(data, data.Length, endPoint);
            return true;
        }

        lock (ConfIdLock) _confId = message.MessageId;                      // If confirmation IS required, store the id of message 
        var tries = 1 + Parameters.MaxRetransmissions;                      // Define the count of tries
        while (tries > 0)                                                   // Try to send the message while count of tries if greater than zero
        {
            _client.Send(data, data.Length, endPoint);                      // Send message
            var result = ConfirmationSemaphore.WaitOne(Parameters.Timeout); // Wait for confirmation or the end of timeout
            if (result)                                                     // If confirmation message was received, reutrn true (success)
                return true;
            tries--;                                                        // Else, decrement the try counter
        }
        return false;                                                       // If all tries were unsuccessful, return false (unsuccess)
    }
    
    // This method implements message receive
    private static void Receiver()
    {
        try
        {
            IPEndPoint? remoteEndPoint = null;                          // Declare new IPEndPoint

            while (true)
            {   
                var receivedData = _client.Receive(ref remoteEndPoint); // Receive message from server
                if (remoteEndPoint.Port != Parameters.Port)             // If server has sent the message from the dynamic port, set it in Parameters
                    lock (Parameters.PortLock) Parameters.Port = (ushort)remoteEndPoint.Port;
                
                ProcessMessageFromServer(receivedData);                 // Process received message
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    
    // This method implements processing of the received message
    private static void ProcessMessageFromServer(byte[] messageData)
    {
        switch (messageData[0]) // The processing algorithm depends on the first byte (type of the received message)
        {
            case 0x00:                                          // Confirmation case
                var confirmationMessage = new UdpConfirm();     // Create a new confirmation message and decode it from received data
                confirmationMessage.DecodeMessage(messageData);
                var refId = confirmationMessage.RefMessageId;
                lock (ConfIdLock)                               // Compare the refId from the received message with the last sent
                    if (_confId == refId)                       // If it is needed refId, release confirmation semaphore (ConnectionFsm continues)
                    {
                        ConfirmationSemaphore.Release();
                        _confId = null;
                    }
                break;
            case 0x01:                                                      // Reply case  
                var replyMessage = new UdpReply();                          // Create new reply message and decode it from received data
                replyMessage.DecodeMessage(messageData);                    
                var confToReplyMessage = new UdpConfirm();                  // Create new confirmation message
                confToReplyMessage.EncodeMessage(replyMessage.MessageId);   // Encode it with received id
                SendMessage(confToReplyMessage, false);                     // Send confirmation
                if (ServerMessageIds.Add(replyMessage.MessageId))           // If this message wasn't received yet, process it
                {
                    Console.WriteLine(replyMessage.MessageContent);         // Print reply message
                    if (ClientFsm.CurrentState == ClientFsm.State.Auth && replyMessage.Result == true) // Process reply based on current client state
                        ClientFsm.CurrentState = ClientFsm.State.Open;
                    WaitForReplySemaphore.Release();                        
                }
                break;
            case 0x04:                                                  // Msg case
                var msgMessage = new UdpMsg();                          // Create new msg message and decode it from received data
                msgMessage.DecodeMessage(messageData);                  
                var confToMsgMessage = new UdpConfirm();                // Create new confirmation message
                confToMsgMessage.EncodeMessage(msgMessage.MessageId);   // Encode it with received id
                SendMessage(confToMsgMessage, false);                   // Send confirmation
                if (ServerMessageIds.Add(msgMessage.MessageId))         // If this message wasn't received yet, print its content
                    Console.WriteLine($"{msgMessage.DisplayName}: {msgMessage.MessageContent}");
                break;
            case 0xFE:                                                  // Err case
                var errMessage = new UdpErr();                          // Create, decode
                errMessage.DecodeMessage(messageData);
                var confToErrMessage = new UdpConfirm();                // Create confirmation message
                SendMessage(confToErrMessage, false);                   // Send confirmation message
                if (ServerMessageIds.Add(errMessage.MessageId))         // If this message wasn' received yet, print its content
                {
                    Console.WriteLine($"{errMessage.DisplayName}: {errMessage.MessageContent}");
                    Terminate();
                }
                break;
            case 0xFF:          // Bye case
                Terminate();    // Terminate connection
                break;
            default:
                Terminate("Bad message format from server");
                break;
        }
    }

    // This method terminated communication safely
    private static void Terminate(string? reason = null)
    {
        var bye = new UdpBye();     // Create bye message
        bye.EncodeMessage();
        SendMessage(bye, false);    // Send bye message
        _client.Close();            // Close UdpClient
        
        if (reason != null)         // Print terminate message if needed
            Console.WriteLine(reason);
        
        Environment.Exit(0);        // Exit
    }
}