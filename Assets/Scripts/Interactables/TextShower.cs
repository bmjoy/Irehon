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
        public override void Interact(Player player)
        {
        }

        public override void StopInterract(Player player)
        {
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