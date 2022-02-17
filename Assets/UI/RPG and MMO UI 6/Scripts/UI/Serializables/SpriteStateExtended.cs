using System;
using UnityEngine;

namespace DuloGames.UI
{
    [Serializable]
    public struct SpriteStateExtended
    {
        //
        // Properties
        //
        [SerializeField] private Sprite m_HighlightedSprite;
        [SerializeField] private Sprite m_PressedSprite;
        [SerializeField] private Sprite m_ActiveSprite;
        [SerializeField] private Sprite m_ActiveHighlightedSprite;
        [SerializeField] private Sprite m_ActivePressedSprite;
        [SerializeField] private Sprite m_DisabledSprite;

        public Sprite highlightedSprite
        {
            get => this.m_HighlightedSprite;
            set => this.m_HighlightedSprite = value;
        }

        public Sprite pressedSprite
        {
            get => this.m_PressedSprite;
            set => this.m_PressedSprite = value;
        }

        public Sprite activeSprite
        {
            get => this.m_ActiveSprite;
            set => this.m_ActiveSprite = value;
        }

        public Sprite activeHighlightedSprite
        {
            get => this.m_ActiveHighlightedSprite;
            set => this.m_ActiveHighlightedSprite = value;
        }

        public Sprite activePressedSprite
        {
            get => this.m_ActivePressedSprite;
            set => this.m_ActivePressedSprite = value;
        }

        public Sprite disabledSprite
        {
            get => this.m_DisabledSprite;
            set => this.m_DisabledSprite = value;
        }
    }
}
