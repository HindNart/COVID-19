using UnityEngine;

public class VirusFactory : MonoBehaviour
{
    [SerializeField] private GameObject basicVirusPrefab;
    [SerializeField] private GameObject omicronVirusPrefab;
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
        int baseHp = 50;
        int basePoints = 10;
        float baseSpeed = 3f;

        if (wave >= 10 && Random.value < 0.3f)
        {
            virusObj = objectPool.Get(omicronVirusPrefab);
            virus = virusObj.GetComponent<OmicronVirus>();
            baseHp *= 2;
            basePoints *= 2;
        }
        else
        {
            virusObj = objectPool.Get(basicVirusPrefab);
            virus = virusObj.GetComponent<BasicVirus>();
        }

        virusObj.transform.position = spawnPos;
        virus.Initialize((int)(baseHp * hpMultiplier), (int)(basePoints * hpMultiplier), baseSpeed * speedMultiplier, player, objectPool);
        return virus;
    }
}