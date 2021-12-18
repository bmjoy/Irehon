using Steamworks;
using System.Threading.Tasks;
using UnityEngine;

namespace Irehon.Chat
{
    public static class SteamDataLoader
    {
        private static Texture2D Covert(Steamworks.Data.Image image)
        {
            // Create a new Texture2D
            Texture2D avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false);

            // Set filter type, or else its really blury
            avatar.filterMode = FilterMode.Trilinear;

            // Flip image
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Steamworks.Data.Color p = image.GetPixel(x, y);
                    avatar.SetPixel(x, (int)image.Height - y, new Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
                }
            }

            avatar.Apply();
            return avatar;
        }

        public static async Task<Sprite> GetSpriteAsync(ulong steamId)
        {
            Steamworks.Data.Image? avatarImage = await SteamFriends.GetSmallAvatarAsync(steamId);
            Texture2D avatarTexture = Covert(avatarImage.Value);
            return Sprite.Create(avatarTexture, new Rect(0, 0, avatarTexture.width, avatarTexture.height), new Vector2(.5f, .5f));
        }

        public static async Task<string> GetNicknameAsync(ulong steamId)
        {
            Friend user = new Friend(steamId);
            await user.RequestInfoAsync();
            return user.Name;
        }
    }
}