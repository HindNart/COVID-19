using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour, IUpgradable
{
    [SerializeField] private GameObject shield;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ObjectPool bulletPool;
    [SerializeField] private float baseFireRate = 0.5f;
    [SerializeField] private int bulletDamage = 10;
    [SerializeField] private float autoFireRadius = 5f;
    [SerializeField] private LayerMask virusLayer;
    [SerializeField] private float retreatDistance = 4f;
    [SerializeField] private float retreatDuration = 0.5f;
    [SerializeField] private float centerReturnDuration = 0.5f;

    private float fireRate;
    private float nextFireTime;
    private int upgradeCost = 100;
    private int upgradeLevel = 1;
    // private int shieldHitsLeft;
    private float speedBoostEndTime;
    private float shieldEndTime;
    private float nextVirusCheckTime;
    private float virusCheckInterval = 0.1f;
    private bool isRetreating;
    private float spinAngle = 0f;
    private Tween scaleTween;

    private void Awake()
    {
        fireRate = baseFireRate;
    }

    private void Start()
    {
        if (shield != null)
        {
            shield.SetActive(false);
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnBossSpawned.AddListener(RetreatFromBoss);
            WaveManager.Instance.OnWaveStarted.AddListener(OnWaveStart);
        }

        GameManager.Instance.OnAntibodiesChanged.AddListener(UpdateUpgradeEffect);
    }

    private void Update()
    {
        // Xoay nhân vật
        Spin();

        // Bắn thủ công khi nhấn chuột
        if (Input.GetMouseButton(0) && Time.time > nextFireTime && !isRetreating)
        {
            if (TryClickPowerUp())
            {
                return;
            }
            else
            {
                Shoot(GetMouseDirection());
                nextFireTime = Time.time + fireRate;
            }
        }
        // Bắn tự động nếu có virus gần
        else if (Time.time > nextFireTime && Time.time > nextVirusCheckTime && !isRetreating)
        {
            Vector2? autoDirection = GetAutoFireDirection();
            if (autoDirection.HasValue)
            {
                Shoot(autoDirection.Value);
                nextFireTime = Time.time + fireRate;
            }
            nextVirusCheckTime = Time.time + virusCheckInterval;
        }

        // Kiểm tra hết thời gian power-up
        if (Time.time > speedBoostEndTime)
        {
            fireRate = baseFireRate / (upgradeLevel * 0.9f);
        }
        if (Time.time > shieldEndTime)
        {
            if (shield != null)
            {
                shield.SetActive(false);
            }
            // shieldHitsLeft = 0;
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

    private void UpdateUpgradeEffect(float antibodies)
    {
        if (CanUpgrade(antibodies) && scaleTween == null)
        {
            scaleTween = transform.DOScale(0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
        else if (!CanUpgrade(antibodies) && scaleTween != null)
        {
            scaleTween.Kill();
            scaleTween = null;
            transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }
    }

    private void OnWaveStart(int wave)
    {
        if (wave % 10 != 0)
        {
            if (!isRetreating)
            {
                StartCoroutine(ReturnToCenterCoroutine());
            }
        }
    }

    private bool TryClickPowerUp()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Player", "PowerUp"));
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                TryUpgrade(GameManager.Instance.Antibodies);
                return true;
            }
            else if (hit.collider.CompareTag("PowerUp"))
            {
                // Kiểm tra xem có trúng PowerUp không
                if (hit.collider.TryGetComponent<PowerUp>(out var powerUp))
                {
                    powerUp.TryActivate(gameObject);
                }
                return true;
            }
        }
        return false;
    }

    private Vector2 GetMouseDirection()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return (mousePos - transform.position).normalized;
    }

    private Vector2? GetAutoFireDirection()
    {
        // Phát hiện virus trong bán kính
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, autoFireRadius, virusLayer);
        if (hits.Length == 0) return null;

        // Tìm virus gần nhất
        Collider2D nearestVirus = null;
        float minDistance = float.MaxValue;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Virus"))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestVirus = hit;
                }
            }
        }

        if (nearestVirus != null)
        {
            return (nearestVirus.transform.position - transform.position).normalized;
        }
        return null;
    }

    private void Shoot(Vector2 direction)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Shoot");
        }

        GameObject bullet = bulletPool.Get(bulletPrefab);
        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        bullet.GetComponent<Bullet>().Initialize(bulletDamage, direction);
    }

    private void RetreatFromBoss()
    {
        if (!isRetreating)
        {
            StartCoroutine(RetreatCoroutine());
        }
    }

    private IEnumerator RetreatCoroutine()
    {
        isRetreating = true;
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + Vector2.left * retreatDistance; // Lùi sang trái
        float elapsedTime = 0f;

        // Giới hạn để không ra ngoài màn hình
        targetPos.y = Mathf.Max(targetPos.y, -8f);

        while (elapsedTime < retreatDuration)
        {
            // transform.position = Vector2.Lerp(startPos, targetPos, elapsedTime / retreatDuration);
            transform.DOMove(targetPos, retreatDuration).SetEase(Ease.OutQuad);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isRetreating = false;
    }

    private IEnumerator ReturnToCenterCoroutine()
    {
        isRetreating = true;
        Vector2 targetPos = Vector2.zero; // Trung tâm (0, 0)
        float elapsedTime = 0f;

        while (elapsedTime < centerReturnDuration)
        {
            // transform.position = Vector2.Lerp(startPos, targetPos, elapsedTime / centerReturnDuration);
            transform.DOMove(targetPos, centerReturnDuration).SetEase(Ease.OutQuad);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isRetreating = false;
    }

    public int GetUpgradeCost()
    {
        return upgradeCost;
    }

    public bool CanUpgrade(float resources)
    {
        return resources >= upgradeCost;
    }

    public void Upgrade()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Upgrade");
        }

        GameManager.Instance.AddAntibodies(-upgradeCost);
        upgradeLevel++;
        bulletDamage += 5;
        fireRate = baseFireRate / (upgradeLevel * 0.9f);
        upgradeCost += 100;
        UpdateUpgradeEffect(GameManager.Instance.Antibodies);
    }

    public void TryUpgrade(float resources)
    {
        if (CanUpgrade(resources))
        {
            Upgrade();
            Debug.Log("Upgrade successful! Current level: " + upgradeLevel);
        }
        else
        {
            Debug.Log("Not enough resources to upgrade. Current resources: " + resources);
        }
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Boost");
        }

        fireRate = baseFireRate / (upgradeLevel * 0.9f * multiplier);
        speedBoostEndTime = Time.time + duration;
    }

    public void ApplyShield(float duration)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Shield");
        }

        if (shield != null)
        {
            shield.SetActive(true);
        }
        // shieldHitsLeft = hits;
        shieldEndTime = Time.time + duration;
    }

    public bool HasShield()
    {
        return shield.activeSelf;
    }

    // public void ConsumeShieldHit()
    // {
    //     if (shieldHitsLeft > 0)
    //     {
    //         shieldHitsLeft--;
    //     }
    // }

    private void OnDrawGizmos()
    {
        // Vẽ bán kính phát hiện virus
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, autoFireRadius);
    }

    private void OnDestroy()
    {
        if (scaleTween != null)
        {
            scaleTween.Kill();
            scaleTween = null;
        }
    }
}