using UnityEngine;
using System.Collections.Generic;

public class PortalSpawner : MonoBehaviour
{
    [SerializeField] 
    private GameObject enemyPrefab;
    [SerializeField] 
    private float spawnInterval = 3f;
    [SerializeField] 
    private float initialSpawnDelay = 0f;
    [SerializeField] 
    private List<Transform> pathWaypoints;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), initialSpawnDelay, spawnInterval);
    }

    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, new Vector3(transform.position.x, 0.08f, transform.position.z), Quaternion.identity);
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        enemyScript.SetPath(pathWaypoints);
    }
}