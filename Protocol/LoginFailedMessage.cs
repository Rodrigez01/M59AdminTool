/*
 Adapted from Meridian59 .NET project
 Copyright (c) 2012-2013 Clint Banzhaf
 Licensed under GPL v3
*/

using System;

namespace M59AdminTool.Protocol
{
    /// <summary>
    /// Sent by the server to the client as negative response to LoginMessage.
    /// </summary>
    [Serializable]
    public class LoginFailedMessage
    {
        public MessageTypeLoginMode MessageType => MessageTypeLoginMode.LoginFailed;

        public LoginFailedMessage()
        {
        }

        /// <summary>
        /// Parse from network buffer
        /// </summary>
        public static LoginFailedMessage Parse(byte[] buffer, int startIndex = 0)
        {
            return new LoginFailedMessage();
        }
    }
}
