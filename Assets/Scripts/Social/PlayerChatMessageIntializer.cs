using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using Steamworks;

public class PlayerChatMessageIntializer : MonoBehaviour
{
    [SerializeField]
    private Image avatar;
    [SerializeField]
    private Text text;
    public async void Intiallize(ulong steamId, string message)
    {
		SetAvatar(steamId);
		var author = new Friend(steamId);
		await author.RequestInfoAsync();
		text.text = $"{author.Name}: {message}";
    }

	public static Texture2D Covert(Steamworks.Data.Image image)
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
				avatar.SetPixel(x, (int)image.Height - y, new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
			}
		}

		avatar.Apply();
		return avatar;
	}

	private async void SetAvatar(ulong steamId)
    {
        var avatarImage = await SteamFriends.GetSmallAvatarAsync(steamId);
		var avatarTexture = Covert(avatarImage.Value);
		avatar.sprite = Sprite.Create(avatarTexture, new Rect(0, 0, avatarTexture.width, avatarTexture.height), new Vector2(.5f, .5f));
    }
}
