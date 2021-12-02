using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using Steamworks;
using System.Threading.Tasks;

public static class SteamUserInformation
{
	private static Texture2D Covert(Steamworks.Data.Image image)
	{
		// Create a new Texture2D
		var avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false);

		// Set filter type, or else its really blury
		avatar.filterMode = FilterMode.Trilinear;

		// Flip image
		for (int x = 0; x < image.Width; x++)
		{
			for (int y = 0; y < image.Height; y++)
			{
				var p = image.GetPixel(x, y);
				avatar.SetPixel(x, (int)image.Height - y, new Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
			}
		}

		avatar.Apply();
		return avatar;
	}

	public static async Task<Sprite> GetSpriteAsync(ulong steamId)
    {
		var avatarImage = await SteamFriends.GetSmallAvatarAsync(steamId);
		var avatarTexture = Covert(avatarImage.Value);
		return Sprite.Create(avatarTexture, new Rect(0, 0, avatarTexture.width, avatarTexture.height), new Vector2(.5f, .5f));
	}

	public static async Task<string> GetNicknameAsync(ulong steamId)
    {
		var user = new Friend(steamId);
		await user.RequestInfoAsync();
		return user.Name;
	}
}

public class PlayerChatMessageIntializer : MonoBehaviour
{
    [SerializeField]
    private Image avatar;
    [SerializeField]
    private Text text;
    public async void Intiallize(ulong steamId, string message)
    {
		text.text = $"{SteamUserInformation.GetNicknameAsync(steamId)}: {message}";
		avatar.sprite = await SteamUserInformation.GetSpriteAsync(steamId);
    }
}
