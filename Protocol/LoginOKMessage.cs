/*
 Adapted from Meridian59 .NET project
 Copyright (c) 2012-2013 Clint Banzhaf
 Licensed under GPL v3
*/

using System;

namespace M59AdminTool.Protocol
{
    /// <summary>
    /// Sent by the server to the client as positive response to LoginMessage.
    /// </summary>
    [Serializable]
    public class LoginOKMessage
    {
        public MessageTypeLoginMode MessageType => MessageTypeLoginMode.LoginOK;

        /// <summary>
        /// Account type of this user (Regular, DM, Admin, etc.)
        /// </summary>
        public byte AccountType { get; set; }

        /// <summary>
        /// Blakserv session ID
        /// </summary>
        public int SessionID { get; set; }

        public LoginOKMessage(byte accountType, int sessionID)
        {
            this.AccountType = accountType;
            this.SessionID = sessionID;
        }

        /// <summary>
        /// Parse from network buffer
        /// </summary>
        public static LoginOKMessage Parse(byte[] buffer, int startIndex = 0)
        {
            int cursor = startIndex;

            byte accountType = buffer[cursor++];
            int sessionID = BitConverter.ToInt32(buffer, cursor);

            return new LoginOKMessage(accountType, sessionID);
        }
    }
}
