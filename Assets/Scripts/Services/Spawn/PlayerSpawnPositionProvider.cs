using UnityEngine;

namespace SoulsLike.Services.Spawn
{
    public class PlayerSpawnPositionProvider : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;

        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;
        public Quaternion SpawnRotation => spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        public void SetSpawnPoint(Transform point)
        {
            spawnPoint = point;
        }
    }
}
