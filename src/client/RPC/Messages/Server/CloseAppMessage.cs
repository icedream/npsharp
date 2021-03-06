using ProtoBuf;

namespace NPSharp.RPC.Messages.Server
{
    [Packet(2001)]
    [ProtoContract]
    public sealed class CloseAppMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public string Reason { get; set; }
    }
}