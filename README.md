# The first IPK project documentation

Documentation for the first IPK project 1: Client for a chat server using  `IPK24-CHAT`  protocol

## Table of Contents

- [The first IPK project documnetation](#the-first-ipk-project-documentation)
	-	[Executive summary](#executive-summary)
	-   [Program execution](#program-execution)
	-   [Implementation](#implementation)
	-   [Bibliography](#bibliography) 

## Executive summary

The program implements a simple client for a chat server. It implements communication with server using two different protocoles:
* The Transmission Control Protocol (TCP) is a transport protocol that is used on top of IP to ensure reliable transmission of packets. TCP includes mechanisms to solve many of the problems that arise from packet-based messaging, such as lost packets, out of order packets, duplicate packets, and corrupted packets.
* The User Datagram Protocol, or UDP, is a communication protocol used across the Internet for especially time-sensitive transmissions such as video playback or DNS lookups. It speeds up communications by not formally establishing a connection before data is transferred.

The program is written in C#, using .net8.0.

## Program execution and usage

The program "ipk24chat-client" accepts the following set of parameters:
* -t {tcp | udp} - sets the type of communication protocole. This parameter is required.
* -s {ip | hostname} - sets the address of the server. This parameter is required.
* -p \{port} - sets the number of port (4567 by default)
* -d \{ms} - sets the timeout for UDP communication in ms (250 by default)
* -r \{count} - sets the maximum number of UDP retrasmissions (3 by default)
* -h - prints the help message and finish the program

Just after the program execution (if CLA parameters were processed successfully) it allows user to print commands in command line. The following set of commands is recognizible:
* /auth \{username} \{secret} \{displayName} - send an `AUTH` message to the server and sets user's display name.
* /join \{channelId} - send `JOIN` message to the server for joining the specified channgel
* /rename \{displayName} - sets new user's display name
* \{message} - sends message to the actual channel

Just at the beginning anly /auth command can be used. After successfull authentication only /join, /rename and message commands can be used.

The client can process incoming message from the server and write them into console.

## Implementation

The program structure is the following:

<img src="./docs/ProgramStructure.svg">

### Common part
This part contains classes which implement the common fuctionalty used in both UDP and TCP variants.
* `Program.cs` - entry point of the whole program, initiate CLA processing and starts the chosen type of communication.
* `Inner/ErrorHandler.cs` - contains a simple error handler class. It can print the error message and exit the environment.
* `Inner/Parameters.cs` - contains class Parameters. It stores the parameters of connection, such as IP address of the server, server port, timeout and so on. It also contains the methods for CLA processing.
* `CheckField.cs` - contains methods for checking the formats of user input data using regular expressions.
* `ClientFsm.cs` - this simple abstract class stores the current state of the connection. There are the following set of possible states:
	1. Auth - user authentication
	1. WaitForAuthReply - waiting for authentication reply from the server
	1. Open - message sending ans receiving
	1. WaitForJoinReply - waiting for join reply from the server
	1. Exit - termination of the connection
	1. End - exit the program

	This class also stores the current user's display name.
* `CommandLine.cs` - this file contains two importand classes:
	i) CommandLine - process commands from user, checks their validity and wraps them into Command objects.
	i) Command - the objects of this class represent the single commands received from the user. Each command has its type and the set of parameters (username, message content, new diplay name and so on).

### UDP and TCP parts
The logic of communication is almost the same for both tcp and udp protocole. This logic is described in the following diagram:

<img src="./docs/ConnectionLogic.svg">

#### UDP variant

UDP communitcation is implemented using the standart UdpClient class. It uses UdpClient.Receive() method for receiving messages from the server and UdpClient.Send for senging.

The UdpMessage class contains the common parameters and methods for all types of UDP messages. Each type of message has its own class (UdpAuth, UdpJoin and so on). All these classes contain EncodeMessage method, or DecodeMessage method, or both of them. EncodeMessage method accepts the set of parameters specific for the needed type of message and encode them into byte array specified in `IPK24-CHAT` protocole. DecodeMessage method accepts an array of bytes and tranform it to the set of parameters.

Every UDP outcoming message (excluding confirmation message) has its own ID generated in UdpClientLogic class.

UdpClientLogic class also contains a HashSet, which is used for storing all IDs of incoming messages from the server.

Sending messages is implemented in SendMessage method of UdpClientLogic class. It accepts an object of UdpMessage class and parameter which indicates if the confirmation message from server is required after sending. Is confirmation is not required, the method will simply send the message and return true (success). But if the confirmation is required, it will send the message and then will try sleep until the confirmation message is received, but not longer than timeout. This behavior is implemented using the semaphore ConfirmationSemaphore. If the receiver thread will receive the confirmation method before the timeout is passed, the method will return true. Else, it will try to send the same message again. If the confirmation message won't be recevied (1+max retrasmissions parameter) times, the method will return false (failed). In this case the program will terminate because of connection lost.

Receiving messages is implementd in Receiver method. It contains an infinite loop. In this loop, the following sequence of actions is implemented:
i) Receive message from the server
ii) If the message has came from the new port, set this new port in Parameters class
iii) Process the message

Message processing is a complex action. It is implemented in ProcessMessageFromServer method. First of all, it determine the type of received message using its first byte. If it is confirmation message, it checks if this confirmation is waited by client and releases the special semaphore ConfirmationSemaphore. If the confirmation is not waits, it will be ignored. If some other type of message was received, the method will send the confirmation message to the server and then will check if the message with the same ID wasn't received before. If it was, message processing is finished. Else, it will process the message due to its type (print the content of MSG message, change ClientFsmState in case of REPLY message with positive result and so on).

#### TCP variant

TCP communication much easier than UDP and is implemented using the standart TcpClient class. It does not require any mechamisms for protecion from packet loss, duplication and delay. It uses StreamReader.ReadLine() for receiving messages and StreamWriter.WriteLine() for sending.

Just like in UDP variant, TCP variant uses special classes for operating with incoming and outcoming messages: TcpMessage, TcpAuth, TcpMsg and others.

The sending and receiving logic is the same as in UDP variant, but does not include confirmation mechanism. 

## Bibliography

Wikipedia, User Datagram Protocol: https://en.wikipedia.org/wiki/User_Datagram_Protocol

Wikipedia, Transmission Control Protocol: https://en.wikipedia.org/wiki/Transmission_Control_Protocol

Microsoft Learn, .NET: https://learn.microsoft.com/en-us/dotnet/