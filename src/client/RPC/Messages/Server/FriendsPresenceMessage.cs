using System;
using NPSharp.RPC.Messages.Data;
using ProtoBuf;

namespace NPSharp.RPC.Messages.Server
{
    [ProtoContract]
    [Packet(1212)]
    public sealed class FriendsPresenceMessage : RPCServerMessage
    {
        internal FriendsPresenceMessage()
        {
        }

        [ProtoMember(1)]
        public UInt64 Friend { get; internal set; }

        [ProtoMember(2)]
        public Int32 PresenceState { get; internal set; }

        [ProtoMember(3)]
        public UInt64 CurrentServer { get; internal set; }

        [ProtoMember(4)]
        public FriendsPresence[] Presence { get; internal set; }
    }
}