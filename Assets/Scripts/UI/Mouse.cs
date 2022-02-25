using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Irehon.UI
{
    public class Mouse : MonoBehaviour
    {
        public static bool IsCursorEnabled { get; private set; }
        public delegate void CursorEventHandler(bool isEnabled);
        public static event CursorEventHandler CursorChanged;

        private void Start()
        {
            DisableCursor();
            UIWindow.ResetWindowsCount();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (!IsCursorEnabled)
                {
                    EnableCursor();
                }
                else
                {
                    DisableCursor();
                }
            }
        }

        public static void EnableCursor()
        {
            Hint.Instance?.HideHint();

            Crosshair.Instance?.DisableCrosshair();

            CursorChanged?.Invoke(true);
            IsCursorEnabled = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public static void DisableCursor()
        {
            Crosshair.Instance?.EnableDefaultCrosshair();
            TooltipWindow.HideTooltip();

            CursorChanged?.Invoke(false);
            IsCursorEnabled = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDestroy()
        {
            EnableCursor();
        }
    }
}