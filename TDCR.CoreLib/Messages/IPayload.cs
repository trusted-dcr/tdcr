using Google.Protobuf;

namespace TDCR.CoreLib.Messages
{
    public interface IPayload<out TWire> where TWire : IMessage
    {
        TWire ToWire();
    }
}
