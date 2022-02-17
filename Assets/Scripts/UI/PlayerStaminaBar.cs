using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irehon.UI
{
    public class PlayerStaminaBar : PlayerStatusBar
    {
        public static PlayerStaminaBar Instance;

        private void Awake() => Instance = this;
    }
}
