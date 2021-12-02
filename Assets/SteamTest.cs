using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using System.IO;

public class SteamTest : MonoBehaviour
{
    [SerializeField]
    private AudioSource source;

    public GameObject gameobject;

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
        SteamClient.Init(570, true);
        SteamUser.VoiceRecord = true;

        optimalRate = 24000;

        clipBufferSize = optimalRate * 5;
        clipBuffer = new float[clipBufferSize];

        stream = new MemoryStream();
        output = new MemoryStream();
        input = new MemoryStream();

        source.clip = AudioClip.Create("VoiceData", (int)256, 1, (int)optimalRate, true, OnAudioRead, null);
        source.loop = true;
        source.Play();
    }

    private void Update()
    {
        SteamClient.RunCallbacks();

        if (SteamUser.HasVoiceData)
        {
            gameobject.SetActive(true);
            print("Have voice data");
            int compressedWritten = SteamUser.ReadVoiceData(stream);
            stream.Position = 0;

            RpcVoiceData(stream.GetBuffer(), compressedWritten);
        }
        print("Voice cycle");
    }

    public void RpcVoiceData(byte[] compressed, int bytesWritten)
    {
        print("Got voice data");
        input.Write(compressed, 0, bytesWritten);
        input.Position = 0;

        int uncompressedWritten = SteamUser.DecompressVoice(input, bytesWritten, output);
        input.Position = 0;

        byte[] outputBuffer = output.GetBuffer();
        WriteToClip(outputBuffer, uncompressedWritten);
        output.Position = 0;
    }

    private void OnAudioRead(float[] data)
    {
        for (int i = 0; i < data.Length; ++i)
        {
            // start with silence
            data[i] = 0;

            // do I  have anything to play?
            if (playbackBuffer > 0)
            {
                // current data position playing
                dataPosition = (dataPosition + 1) % clipBufferSize;

                data[i] = clipBuffer[dataPosition];

                playbackBuffer--;
            }
        }

    }

    private void WriteToClip(byte[] uncompressed, int iSize)
    {
        for (int i = 0; i < iSize; i += 2)
        {
            // insert converted float to buffer
            float converted = (short)(uncompressed[i] | uncompressed[i + 1] << 8) / 32767.0f;
            clipBuffer[dataReceived] = converted;

            // buffer loop
            dataReceived = (dataReceived + 1) % clipBufferSize;

            playbackBuffer++;
        }
    }

    private void OnDisable()
    {
        SteamUser.VoiceRecord = false;
    }
}
