using Irehon.Steam;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Irehon.UI
{
    public class PlayerStatusFrame : MonoBehaviour
    {
        [SerializeField]
        private Image avatarFrame;

        private async void Start()
        {
            avatarFrame.sprite = await SteamDataLoader.GetSpriteAsync(Steamworks.SteamClient.SteamId);
        }
    }
}