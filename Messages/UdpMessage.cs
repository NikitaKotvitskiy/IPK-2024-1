namespace IPK_2024_1.Messages
{
    internal abstract class UdpMessage
    {
        public byte[]? Data { get; protected set; }

        public ushort? MessageId { get; protected set; }
        public ushort? RefMessageId { get; protected set; }
        public string? Username { get; protected set; }
        public string? ChannelId { get; protected set; }
        public string? Secret { get; protected set; }
        public string? DisplayName { get; protected set; }
        public string? MessageContent { get; protected set; }
        public bool? Result { get; protected set; }

        public abstract void DecodeMessage(byte[] data);

        protected int DefineStringLength(byte[] data, ref int startPosition)
        {
            var length = 0;
            while (data[startPosition] != 0)
            {
                length++;
                startPosition++;
            }
            startPosition++;
            return length;
        }

        public void DebugPrintMessage()
        {
            if (Data != null)
                foreach (var b in Data)
                    Console.Write($"{b:X2} ");
            Console.WriteLine();
            if (MessageId != null)
                Console.WriteLine($"Message ID: {MessageId}");
            if (RefMessageId != null)
                Console.WriteLine($"Reference message ID: {RefMessageId}");
            if (Username != null)
                Console.WriteLine($"Username: {Username}");
            if (ChannelId != null)
                Console.WriteLine($"Channel ID: {ChannelId}");
            if (Secret != null)
                Console.WriteLine($"Secret: {Secret}");
            if (DisplayName != null)
                Console.WriteLine($"Display name: {DisplayName}");
            if (MessageContent != null)
                Console.WriteLine($"Message content: {MessageContent}");
            if (Result != null) 
                Console.WriteLine($"Result: {Result}");
        }
    }
}
