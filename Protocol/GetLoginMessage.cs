/*
 Adapted from Meridian59 .NET project
 Copyright (c) 2012-2013 Clint Banzhaf
 Licensed under GPL v3
*/

using System;

namespace M59AdminTool.Protocol
{
    /// <summary>
    /// First message sent from server to client requesting login credentials.
    /// This is a simple message with no payload - just the message type.
    /// </summary>
    [Serializable]
    public class GetLoginMessage
    {
        public MessageTypeLoginMode MessageType => MessageTypeLoginMode.GetLogin;

        public GetLoginMessage()
        {
        }

        /// <summary>
        /// Parse from network buffer
        /// </summary>
        public static GetLoginMessage Parse(byte[] buffer, int startIndex = 0)
        {
            return new GetLoginMessage();
        }
    }
}
