using UnityEngine;

public class VirusFactory : MonoBehaviour
{
    [SerializeField] private GameObject basicVirusPrefab;
    [SerializeField] private GameObject intermediateVirusPrefab;
    [SerializeField] private GameObject advancedVirusPrefab;
    [SerializeField] private GameObject omicronVirusPrefab;
    [SerializeField] private GameObject[] bossVirusPrefabs;
    [SerializeField] private Transform player;
    [SerializeField] private ObjectPool objectPool;

    private float hpMultiplier = 1f;
    private float speedMultiplier = 1f;

    public void SetWaveParameters(float hpMult, float speedMult)
    {
        hpMultiplier = hpMult;
        speedMultiplier = speedMult;
    }

    public VirusBase SpawnVirus(int wave, Vector2 spawnPos)
    {
        GameObject virusObj;
        VirusBase virus;

        int baseHp = 10;
        float basePoints = 1;
        float baseSpeed = 0.5f;

        // wave boss
        if (wave % 10 == 0)
        {
            virusObj = objectPool.Get(bossVirusPrefabs[Random.Range(0, bossVirusPrefabs.Length)]);
            virus = virusObj.GetComponent<BossVirus>();
            baseHp *= 50;
            basePoints *= 2f;
            baseSpeed *= 0.25f;
        }
        // omicron virus
        else if (wave > 10 && Random.value < 0.15f)
        {
            virusObj = objectPool.Get(omicronVirusPrefab);
            virus = virusObj.GetComponent<OmicronVirus>();
            baseHp *= 2;
            basePoints *= 1.3f;
        }
        // advanced virus
        else if (wave > 15 && Random.value < 0.3f)
        {
            virusObj = objectPool.Get(advancedVirusPrefab);
            virus = virusObj.GetComponent<BasicVirus>();
            baseHp *= 3;
            basePoints *= 1.3f;
        }
        // intermediate virus
        else if (wave > 5 && Random.value < 0.5f)
        {
            virusObj = objectPool.Get(intermediateVirusPrefab);
            virus = virusObj.GetComponent<BasicVirus>();
            baseHp *= 2;
            basePoints *= 1.1f;
        }
        // basic virus
        else
        {
            virusObj = objectPool.Get(basicVirusPrefab);
            virus = virusObj.GetComponent<BasicVirus>();
        }

        virusObj.transform.position = spawnPos;
        virus.Initialize((int)(baseHp * hpMultiplier), basePoints * (hpMultiplier / 3f), baseSpeed * speedMultiplier, player, objectPool);
        return virus;
    }
}