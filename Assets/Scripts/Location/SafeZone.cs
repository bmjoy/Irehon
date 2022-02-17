using Irehon.Entitys;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon
{
    public class SafeZone : MonoBehaviour
    {
        public Fraction safeFraction;
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("EntityBase"))
            {
                return;
            }

            print($"{other.name} {other.gameObject.name}");
            Player player = other.GetComponent<Player>();
            if (player == null || !player.isServer || player.fraction != safeFraction)
            {
                return;
            }

            if (!player.takeDamageProcessQuerry.Contains(PvpTakeDamageBlock))
                player.takeDamageProcessQuerry.Add(PvpTakeDamageBlock);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("EntityBase"))
            {
                return;
            }

            print($"{other.name} {other.gameObject.name}");
            Player player = other.GetComponent<Player>();
            if (player == null || !player.isServer || player.fraction != safeFraction)
            {
                return;
            }

            if (player.takeDamageProcessQuerry.Contains(PvpTakeDamageBlock))
                player.takeDamageProcessQuerry.Remove(PvpTakeDamageBlock);
        }

        private void PvpTakeDamageBlock(ref DamageMessage damageMessage)
        {
            if (damageMessage.source is Player)
            {
                damageMessage.damage = 0;
            }
        }
    }
}