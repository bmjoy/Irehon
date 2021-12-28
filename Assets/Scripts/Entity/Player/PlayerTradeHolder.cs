using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon
{
    [RequireComponent(typeof(Player))]
    public class PlayerTradeHolder : NetworkBehaviour
    {
        private Player player;

        private void Start()
        {
            player = GetComponent<Player>();
        }


    }
}