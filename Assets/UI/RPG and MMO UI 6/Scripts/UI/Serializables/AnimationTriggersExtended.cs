using System;
using UnityEngine;

namespace DuloGames.UI
{
    [Serializable]
    public class AnimationTriggersExtended
    {
        //
        // Properties
        //
        [SerializeField] private string m_NormalTrigger = "Normal";
        [SerializeField] private string m_HighlightedTrigger = "Highlighted";
        [SerializeField] private string m_PressedTrigger = "Pressed";
        [SerializeField] private string m_ActiveTrigger = "Active";
        [SerializeField] private string m_ActiveHighlightedTrigger = "ActiveHighlighted";
        [SerializeField] private string m_ActivePressedTrigger = "ActivePressed";
        [SerializeField] private string m_DisabledTrigger = "Disabled";

        public string normalTrigger
        {
            get => this.m_NormalTrigger;
            set => this.m_NormalTrigger = value;
        }

        public string highlightedTrigger
        {
            get => this.m_HighlightedTrigger;
            set => this.m_HighlightedTrigger = value;
        }

        public string pressedTrigger
        {
            get => this.m_PressedTrigger;
            set => this.m_PressedTrigger = value;
        }

        public string activeTrigger
        {
            get => this.m_ActiveTrigger;
            set => this.m_ActiveTrigger = value;
        }

        public string activeHighlightedTrigger
        {
            get => this.m_ActiveHighlightedTrigger;
            set => this.m_ActiveHighlightedTrigger = value;
        }

        public string activePressedTrigger
        {
            get => this.m_ActivePressedTrigger;
            set => this.m_ActivePressedTrigger = value;
        }

        public string disabledTrigger
        {
            get => this.m_DisabledTrigger;
            set => this.m_DisabledTrigger = value;
        }
    }
}
