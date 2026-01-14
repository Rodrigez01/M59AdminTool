/*
 Adapted from Meridian59 .NET project
 Copyright (c) 2012-2013 Clint Banzhaf
 Licensed under GPL v3
*/

namespace M59AdminTool.Protocol
{
    /// <summary>
    /// Valid message types for protocol mode 'login'.
    /// See original 'proto.h' in Meridian 59 source.
    /// </summary>
    public enum MessageTypeLoginMode : byte
    {
        Ping            = 1,
        Login           = 2,
        Register        = 3,
        ReqGame         = 4,
        ReqAdmin        = 5,
        Resync          = 6,
        GetClient       = 7,
        GetResource     = 8,
        GetAll          = 9,
        ReqMenu         = 10,
        AdminNote       = 11,
        ClientPatchOld  = 12,
        ClientPatch     = 13,

        GetLogin        = 21,
        GetChoice       = 22,
        LoginOK         = 23,
        LoginFailed     = 24,
        Game            = 25,
        Admin           = 26,
        AccountUsed     = 27,
        TooManyLogins   = 28,
        Timeout         = 29,
        Credits         = 30,
        Download        = 31,
        Upload          = 32,
        NoCredits       = 33,
        Message         = 34,
        DeleteRsc       = 35,
        DeleteAllRsc    = 36,
        NoCharacters    = 37,
        Guest           = 38,
        ServiceReport   = 39
    }
}
