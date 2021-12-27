using Mirror;
using Steamworks;
using System.IO;
using UnityEngine;

namespace Irehon.Voice
{
    public class VoiceChat : NetworkBehaviour
    {
        [SerializeField]
        private AudioSource source;

        private MemoryStream output;
        private MemoryStream stream;
        private MemoryStream input;

        private int optimalRate;
        private int clipBufferSize;
        private float[] clipBuffer;

        private int playbackBuffer;
        private int dataPosition;
        private int dataReceived;

        private void Start()
        {
            if (isClient)
            {
                this.optimalRate = 24000;

                this.clipBufferSize = this.optimalRate * 5;
                this.clipBuffer = new float[this.clipBufferSize];

                this.stream = new MemoryStream();
                this.output = new MemoryStream();
                this.input = new MemoryStream();
                this.source.clip = AudioClip.Create("VoiceData", 256, 1, this.optimalRate, true, this.OnAudioRead, null);
                this.source.loop = true;
                this.source.Play();
            }
        }

        [ClientCallback]
        private void Update()
        {
            if (!this.isLocalPlayer)
            {
                return;
            }

            SteamUser.VoiceRecord = Input.GetKey(KeyCode.F);

            if (SteamUser.HasVoiceData)
            {
                int compressedWritten = SteamUser.ReadVoiceData(this.stream);
                this.stream.Position = 0;

                this.CmdVoice(this.stream.GetBuffer(), compressedWritten);
            }
        }

        [Command]
        public void CmdVoice(byte[] compressed, int bytesWritten)
        {
            if (compressed != null)
            {
                this.RpcVoiceData(compressed, bytesWritten);
            }
        }


        [ClientRpc(includeOwner = false)]
        public void RpcVoiceData(byte[] compressed, int bytesWritten)
        {
            this.input.Write(compressed, 0, bytesWritten);
            this.input.Position = 0;

            int uncompressedWritten = SteamUser.DecompressVoice(this.input, bytesWritten, this.output);
            this.input.Position = 0;

            byte[] outputBuffer = this.output.GetBuffer();
            this.WriteToClip(outputBuffer, uncompressedWritten);
            this.output.Position = 0;
        }

        [Client]
        private void OnAudioRead(float[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                // start with silence
                data[i] = 0;

                // do I  have anything to play?
                if (this.playbackBuffer > 0)
                {
                    // current data position playing
                    this.dataPosition = (this.dataPosition + 1) % this.clipBufferSize;

                    data[i] = this.clipBuffer[this.dataPosition] * 14;

                    this.playbackBuffer--;
                }
            }

        }

        [Client]
        private void WriteToClip(byte[] uncompressed, int iSize)
        {
            for (int i = 0; i < iSize; i += 2)
            {
                // insert converted float to buffer
                float converted = (short)(uncompressed[i] | uncompressed[i + 1] << 8) / 32767.0f;
                this.clipBuffer[this.dataReceived] = converted;

                // buffer loop
                this.dataReceived = (this.dataReceived + 1) % this.clipBufferSize;

                this.playbackBuffer++;
            }
        }
    }
}