using UnityEngine;
using UnityEngine.UI;

namespace Irehon.Chat
{
    public class Message : MonoBehaviour
    {
        [SerializeField]
        private Image avatar;
        [SerializeField]
        private Text text;
        public async void Intiallize(ulong steamId, string message)
        {
            string nickname = await SteamDataLoader.GetNicknameAsync(steamId);
            this.text.text = $"{nickname}: {message}";
            this.avatar.sprite = await SteamDataLoader.GetSpriteAsync(steamId);
        }
    }
}