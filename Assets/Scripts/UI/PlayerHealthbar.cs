using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Irehon.UI
{
    public class PlayerHealthBar : PlayerStatusBar
    {
        public static PlayerHealthBar Instance;

        private void Awake() => Instance = this; 
    }
}
