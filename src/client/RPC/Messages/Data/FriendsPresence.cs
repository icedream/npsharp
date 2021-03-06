using ProtoBuf;

namespace NPSharp.RPC.Messages.Data
{
    [ProtoContract]
    public sealed class FriendsPresence
    {
        internal FriendsPresence()
        {
        }

        [ProtoMember(1)]
        public string Key { get; set; }

        [ProtoMember(2)]
        public string Value { get; set; }
    }
}