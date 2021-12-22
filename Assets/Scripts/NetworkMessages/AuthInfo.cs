using Mirror;

namespace Irehon
{
    public struct AuthInfo : NetworkMessage
    {
        public ulong Id;
        public byte[] AuthData;
        public RegisterInfo registerInfo;
    }
}