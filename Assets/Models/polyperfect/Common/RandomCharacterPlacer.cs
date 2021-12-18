using UnityEngine;
using UnityEngine.AI;

namespace PolyPerfect
{
    [ExecuteInEditMode]
    public class RandomCharacterPlacer : MonoBehaviour
    {
        [SerializeField] private float spawnSize;
        [SerializeField] private int spawnAmmount;

        [SerializeField] private GameObject[] characters;

        [ContextMenu("Spawn Characters")]
        private void SpawnAnimals()
        {
            GameObject parent = new GameObject("SpawnedCharacters");

            for (int i = 0; i < this.spawnAmmount; i++)
            {
                int value = Random.Range(0, this.characters.Length);

                Instantiate(this.characters[value], this.RandomNavmeshLocation(this.spawnSize), Quaternion.identity, parent.transform);
            }
        }

        public Vector3 RandomNavmeshLocation(float radius)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += this.transform.position;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(this.transform.position, this.spawnSize);
        }
    }
}
