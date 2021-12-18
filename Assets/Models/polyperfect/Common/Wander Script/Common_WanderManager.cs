using UnityEngine;

namespace PolyPerfect
{
    public class Common_WanderManager : MonoBehaviour
    {
        [SerializeField]
        private bool peaceTime;
        public bool PeaceTime
        {
            get => this.peaceTime;
            set => this.SwitchPeaceTime(value);
        }

        private static Common_WanderManager instance;
        public static Common_WanderManager Instance => instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
        }

        private void Start()
        {
            if (this.peaceTime)
            {
                Debug.Log("AnimalManager: Peacetime is enabled, all animals are non-agressive.");
                this.SwitchPeaceTime(true);
            }
        }

        public void SwitchPeaceTime(bool enabled)
        {
            if (enabled == this.peaceTime)
            {
                return;
            }

            this.peaceTime = enabled;

            Debug.Log(string.Format("AnimalManager: Peace time is now {0}.", enabled ? "On" : "Off"));
            foreach (Common_WanderScript animal in Common_WanderScript.AllAnimals)
            {
                animal.SetPeaceTime(enabled);
            }
        }

        public void Nuke()
        {
            foreach (Common_WanderScript animal in Common_WanderScript.AllAnimals)
            {
                animal.Die();
            }
        }
    }
}