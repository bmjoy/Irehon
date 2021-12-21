using Mirror;

namespace Irehon
{
    public struct AuthRequestMessage : NetworkMessage
    {
        public ulong Id;
        public byte[] AuthData;
        public RegisterInfo registerInfo;
    }
}