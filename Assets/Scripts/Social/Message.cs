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

        private bool isDissapearing;
        private float dissapearTimer;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            ChatWindow.Instance.ChatActiveStateChanged += OnChatOpened;
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        private void Update()
        {
            if (isDissapearing)
            {
                if (dissapearTimer > 0)
                    dissapearTimer -= Time.deltaTime;
                if (dissapearTimer < 1 && dissapearTimer > 0)
                {
                    canvasGroup.alpha = dissapearTimer;
                }
                if (dissapearTimer < 0)
                {
                    canvasGroup.alpha = 1;
                    isDissapearing = false;
                    gameObject.SetActive(false);
                }
            }
        }

        public async void Intiallize(ulong steamId, string message)
        {
            dissapearTimer = 4f;
            isDissapearing = true;
            string nickname = await SteamDataLoader.GetNicknameAsync(steamId);
            this.text.text = $"{nickname}: {message}";
            this.avatar.sprite = await SteamDataLoader.GetSpriteAsync(steamId);
        }

        private void OnChatOpened(bool isOpened)
        {
            if (isOpened)
            {
                gameObject.SetActive(true);
                isDissapearing = false;
            }
            else
            {
                isDissapearing = true;
                dissapearTimer = 3f;
            }
        }

        private void OnDestroy()
        {
            ChatWindow.Instance.ChatActiveStateChanged -= OnChatOpened;
        }
    }
}