using System;
using UnityEngine;

namespace DuloGames.UI
{
    public class UITooltipManager : ScriptableObject
    {
        #region singleton
        private static UITooltipManager m_Instance;
        public static UITooltipManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = Resources.Load("TooltipManager") as UITooltipManager;
                }

                return m_Instance;
            }
        }
        #endregion

#pragma warning disable 0649
        [SerializeField] private GameObject m_TooltipPrefab;
#pragma warning restore 0649

        /// <summary>
        /// Gets the tooltip prefab.
        /// </summary>
        public GameObject prefab => this.m_TooltipPrefab;

        [SerializeField] private int m_SpacerHeight = 6;
        [SerializeField] private int m_ItemTooltipWidth = 514;
        [SerializeField] private int m_SpellTooltipWidth = 514;

        /// <summary>
        /// Spacer height used for the spacer line.
        /// </summary>
        public int spacerHeight => this.m_SpacerHeight;

        /// <summary>
        /// The width used for the item tooltip.
        /// </summary>
        public int itemTooltipWidth => this.m_ItemTooltipWidth;

        /// <summary>
        /// The width used for the spell tooltip.
        /// </summary>
        public int spellTooltipWidth => this.m_SpellTooltipWidth;

        [Header("Styles")]
        [SerializeField] private UITooltipLineStyle m_DefaultLineStyle = new UITooltipLineStyle(false);
        [SerializeField] private UITooltipLineStyle m_TitleLineStyle = new UITooltipLineStyle(false);
        [SerializeField] private UITooltipLineStyle m_DescriptionLineStyle = new UITooltipLineStyle(false);
        [SerializeField] private UITooltipLineStyle[] m_CustomStyles = new UITooltipLineStyle[0];

        /// <summary>
        /// Default line style used when no style is specified.
        /// </summary>
        public UITooltipLineStyle defaultLineStyle => this.m_DefaultLineStyle;

        /// <summary>
        /// Title line style used for the tooltip title.
        /// </summary>
        public UITooltipLineStyle titleLineStyle => this.m_TitleLineStyle;

        /// <summary>
        /// Description line style used for the description.
        /// </summary>
        public UITooltipLineStyle descriptionLineStyle => this.m_DescriptionLineStyle;

        /// <summary>
        /// The custom styles array.
        /// </summary>
        public UITooltipLineStyle[] customStyles => this.m_CustomStyles;

        /// <summary>
        /// Gets a custom style by the specified name.
        /// </summary>
        /// <param name="name">The custom style name.</param>
        /// <returns>The custom style or the default style if not found.</returns>
        public UITooltipLineStyle GetCustomStyle(string name)
        {
            if (this.m_CustomStyles.Length > 0)
            {
                foreach (UITooltipLineStyle style in this.m_CustomStyles)
                {
                    if (style.Name.Equals(name))
                    {
                        return style;
                    }
                }
            }

            return this.m_DefaultLineStyle;
        }

        [ContextMenu("Sort Custom Styles")]
        public void SortCustomStyles()
        {
            Array.Sort<UITooltipLineStyle>(this.m_CustomStyles);
        }
    }
}
