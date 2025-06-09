using UnityEngine;

public class VirussSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject virusPrefab;
    [SerializeField] private Vector2 spawnWith = new();
    [SerializeField] private float spawnRate = 2f;
    private float spawnTimer = 0f;

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnRate)
        {
            SpawnVirus();
            spawnTimer = 0f;
        }
        spawnRate -= 0.000035f;
        if (spawnRate < 1)
        {
            spawnRate = 1;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, spawnWith);
    }

    void SpawnVirus()
    {
        if (virusPrefab != null)
        {
            Vector2 randomPosition;
            if (transform.position.x != 0 && transform.position.y == 0)
                randomPosition = (Vector2)transform.position + new Vector2(0, Random.Range(-spawnWith.y / 2, spawnWith.y / 2));
            else
                randomPosition = (Vector2)transform.position + new Vector2(Random.Range(-spawnWith.x / 2, spawnWith.x / 2), 0);
            Instantiate(virusPrefab, randomPosition, Quaternion.identity);
        }
    }
}
