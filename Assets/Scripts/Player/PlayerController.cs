using UnityEngine;

public class PlayerController : MonoBehaviour, IUpgradable
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ObjectPool bulletPool;
    [SerializeField] private float baseFireRate = 0.5f;
    [SerializeField] private int bulletDamage = 10;
    private float fireRate;
    private float nextFireTime;
    private int upgradeCost = 100;
    private int upgradeLevel = 1;
    private int shieldHitsLeft;
    private float speedBoostEndTime;
    private float shieldEndTime;
    private float spinAngle = 0f;

    private void Awake()
    {
        fireRate = baseFireRate;
    }

    private void Update()
    {
        Spin();

        if (Input.GetMouseButton(0) && Time.time > nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        if (Time.time > speedBoostEndTime)
        {
            fireRate = baseFireRate / (upgradeLevel * 0.9f);
        }
        if (Time.time > shieldEndTime)
        {
            shieldHitsLeft = 0;
        }
    }

    private void Spin()
    {
        spinAngle -= 0.05f;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, spinAngle));
        if (spinAngle <= -360f)
        {
            spinAngle = 0f;
        }
    }

    private void Shoot()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3 direction = (mousePos - transform.position).normalized;

        GameObject bullet = bulletPool.Get(bulletPrefab);
        bullet.transform.position = transform.position;
        bullet.GetComponent<Bullet>().Initialize(bulletDamage, direction);
    }

    public bool CanUpgrade(int resources)
    {
        return resources >= upgradeCost;
    }

    public void Upgrade()
    {
        GameManager.Instance.AddAntibodies(-upgradeCost);
        upgradeLevel++;
        bulletDamage += 10;
        fireRate = baseFireRate / (upgradeLevel * 0.9f);
        upgradeCost += 50;
    }

    public void TryUpgrade(int resources)
    {
        if (CanUpgrade(resources))
        {
            Upgrade();
        }
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        fireRate = baseFireRate / (upgradeLevel * 0.9f * multiplier);
        speedBoostEndTime = Time.time + duration;
    }

    public void ApplyShield(int hits, float duration)
    {
        shieldHitsLeft = hits;
        shieldEndTime = Time.time + duration;
    }

    public bool HasShield()
    {
        return shieldHitsLeft > 0;
    }

    public void ConsumeShieldHit()
    {
        if (shieldHitsLeft > 0)
        {
            shieldHitsLeft--;
        }
    }
}