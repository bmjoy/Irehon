using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using Irehon.UI;

namespace Irehon.Interactable
{
    public class TextShower : Interactable
    {
        [SerializeField]
        private string header;
        [SerializeField]
        private string text;
        [SerializeField]
        private int font = 20;
        [SerializeField]
        private Color color;

        public override void Interact(Player player)
        {
            TargetOpenTextWindow(player.connectionToClient, header, text, font, color);
        }

        public override void StopInterract(Player player)
        {
            TargetCloseTextWindow(player.connectionToClient);
        }

        [TargetRpc]
        private void TargetOpenTextWindow(NetworkConnection target, string header, string text, int font, Color color)
        {
            TextWindow.Instance.ShowText(header, text, font, color);
        }

        [TargetRpc]
        private void TargetCloseTextWindow(NetworkConnection target)
        {
            TextWindow.Instance.CloseTextWindow();
        }
    }
}