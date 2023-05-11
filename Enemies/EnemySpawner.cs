using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject enemySpawnPoints;
    private Transform[] points;

    private void Awake() {
        if (enemySpawnPoints != null) {
            points = enemySpawnPoints.GetComponentsInChildren<Transform>(false);
            points = points.Where(t => t != enemySpawnPoints.transform).ToArray();
        }
    }

    private void Start() {
        SpawnEnemies();
    }


    public void SpawnEnemies() {
        foreach (Transform point in points) {
            Instantiate(enemyPrefab, point.position, point.rotation);
        }
    }

}
