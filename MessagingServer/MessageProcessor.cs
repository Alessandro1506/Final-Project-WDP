using System;
using System.Collections.Generic;
using System.Text;

namespace MessagingServer
{
    public class ChatMessage
    {
        public readonly String FromUser;
        public readonly String ToUser;
        public readonly DateTime TimeStamp;
        public readonly String Text;

        public ChatMessage(String fromUser, String toUser, DateTime timeStamp, String text)
        {
            this.FromUser = fromUser;
            this.ToUser = toUser;
            this.TimeStamp = timeStamp;
            this.Text = text;
            return;
        }
    }

    public class MessageProcessor
    {
        private readonly Dictionary<String, List<ChatMessage>> messageQueue;
        private readonly Object queueLock;
        private readonly Logger logger;

        public MessageProcessor(Logger loggerInstance)
        {
            Dictionary<String, List<ChatMessage>> queue;
            Object lockObject;

            this.logger = loggerInstance;
            queue = new Dictionary<String, List<ChatMessage>>();
            lockObject = new Object();

            this.messageQueue = queue;
            this.queueLock = lockObject;
            return;
        }

        public String ProcessMessage(String requestText)
        {
            String response;
            String[] parts;
            String command;
            Boolean handled;

            response = "ERR|InvalidRequest";
            command = String.Empty;
            handled = false;
            parts = null;

            if (!String.IsNullOrWhiteSpace(requestText))
            {
                parts = requestText.Split('|');

                if (parts.Length > 0)
                {
                    command = parts[0];

                    if (command == "SEND" && parts.Length >= 4)
                    {
                        response = this.ProcessSend(parts[1], parts[2], parts[3]);
                        handled = true;
                    }
                    else if (command == "PULL" && parts.Length >= 2)
                    {
                        response = this.ProcessPull(parts[1]);
                        handled = true;
                    }
                }
            }

            if (!handled)
            {
                Logger.WriteLog(this.logger.GetLogPath(), "Invalid request: " + requestText);
            }

            return response;
        }

        private String ProcessSend(String fromUser, String toUser, String text)
        {
            String response;
            ChatMessage message;
            List<ChatMessage> list;

            response = "ACK|MessageQueued";
            message = new ChatMessage(fromUser, toUser, DateTime.Now, text);
            list = null;

            lock (this.queueLock)
            {
                if (!this.messageQueue.ContainsKey(toUser))
                {
                    this.messageQueue[toUser] = new List<ChatMessage>();
                }

                list = this.messageQueue[toUser];
                list.Add(message);
            }

            Logger.WriteLog(this.logger.GetLogPath(), "Message queued from " + fromUser + " to " + toUser);
            return response;
        }

        private String ProcessPull(String userName)
        {
            String result;
            List<ChatMessage> messagesForUser;
            Boolean hasMessages;
            StringBuilder builder;
            Int32 i;
            ChatMessage message;
            String formatted;

            result = "NOMSG";
            messagesForUser = null;
            hasMessages = false;
            builder = new StringBuilder();

            lock (this.queueLock)
            {
                if (this.messageQueue.ContainsKey(userName))
                {
                    messagesForUser = this.messageQueue[userName];

                    if (messagesForUser != null && messagesForUser.Count > 0)
                    {
                        hasMessages = true;
                    }

                    this.messageQueue[userName] = new List<ChatMessage>();
                }
            }

            if (hasMessages && messagesForUser != null)
            {
                for (i = 0; i < messagesForUser.Count; i++)
                {
                    message = messagesForUser[i];
                    formatted = "MSG|" + message.FromUser + "|" + message.TimeStamp.ToString("o") + "|" + message.Text;

                    if (builder.Length > 0)
                    {
                        builder.Append("~");
                    }

                    builder.Append(formatted);
                }

                result = builder.ToString();
                Logger.WriteLog(this.logger.GetLogPath(), "Returned " + messagesForUser.Count.ToString() + " messages for " + userName);
            }
            else
            {
                Logger.WriteLog(this.logger.GetLogPath(), "No messages for " + userName);
            }

            return result;
        }
    }
}
