using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class BossVirus : VirusBase
{
    // [SerializeField] private GameObject bossBulletPrefab;
    public static UnityEvent onBossDefeated = new UnityEvent();
    [SerializeField] private GameObject minionPrefab; // BasicVirus prefab
    // [SerializeField] private float shootInterval = 3f;
    [SerializeField] private float minionSpawnInterval = 15f;
    // [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float lowHPThreshold = 0.3f;
    // private float nextShootTime;
    private float nextMinionSpawnTime;
    private int MaxHP => hp; // Lấy max HP từ VirusBase
    private SpriteRenderer spriteRenderer;
    private Tween flashTween;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Lấy SpriteRenderer
    }

    public override void Initialize(int health, float reward, float spd, Transform tgt, ObjectPool objectPool)
    {
        base.Initialize(health, reward, spd, tgt, objectPool);
        // nextShootTime = Time.time + shootInterval;
        nextMinionSpawnTime = Time.time + minionSpawnInterval;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    protected override void Update()
    {
        base.Update();

        // Bắn đạn
        // if (Time.time >= nextShootTime && target != null)
        // {
        //     Shoot();
        //     nextShootTime = Time.time + shootInterval;
        // }
    }

    protected override void Move()
    {
        if (target == null) return;

        float stopDistance = 6f;
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > stopDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
        else
        {
            // Triệu hồi minion
            if (Time.time >= nextMinionSpawnTime)
            {
                StartCoroutine(SpawnMinions());
                nextMinionSpawnTime = Time.time + minionSpawnInterval;
            }
        }
    }

    // private void Shoot()
    // {
    //     if (pool == null || bossBulletPrefab == null) return;

    //     GameObject bullet = pool.Get(bossBulletPrefab);
    //     bullet.transform.position = transform.position;
    //     Vector2 direction = (target.position - transform.position).normalized;
    //     bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
    //     bullet.GetComponent<BossBullet>().Initialize(20, direction, bulletSpeed); // Sát thương 20
    // }

    private IEnumerator SpawnMinions()
    {
        if (pool == null || minionPrefab == null) yield return null;

        int minionCount = Random.Range(10, 15); // 10-15 minion
        for (int i = 0; i < minionCount; i++)
        {
            GameObject minion = pool.Get(minionPrefab);
            Vector2 spawnOffset = Random.insideUnitCircle * 3f;
            minion.transform.position = (Vector2)transform.position + spawnOffset;
            minion.GetComponent<BasicVirus>().Initialize(50, 1, 1.5f, target, pool); // Minion yếu
            yield return new WaitForSeconds(1f); // Thời gian giữa các lần triệu hồi
        }
    }

    public new void TakeDamage(int damage)
    {
        hp -= damage;

        // Kiểm tra HP để kích hoạt/hủy nhấp nháy
        float hpRatio = (float)hp / MaxHP;
        if (hpRatio <= lowHPThreshold && flashTween == null)
        {
            StartFlashing();
        }
        else if (hpRatio > lowHPThreshold && flashTween != null)
        {
            StopFlashing();
        }

        if (hp <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        // Hủy nhấp nháy trước khi chết
        StopFlashing();

        base.Die();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("BossDeath");
        }

        // Thông báo Boss bị tiêu diệt
        onBossDefeated?.Invoke();
    }

    private void StartFlashing()
    {
        if (spriteRenderer == null) return;

        // Tạo Tween nhấp nháy
        flashTween = spriteRenderer.DOFade(0.5f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopFlashing()
    {
        if (flashTween != null)
        {
            flashTween.Kill(); // Hủy Tween
            flashTween = null;
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // Reset alpha
        }
    }

    private void OnDisable()
    {
        // Hủy Tween khi Boss bị vô hiệu hóa
        StopFlashing();
    }
}

// Script cho đạn của Boss
// public class BossBullet : MonoBehaviour
// {
//     private int damage;
//     private Vector2 direction;
//     private float speed;
//     private ObjectPool pool;

//     public void Initialize(int dmg, Vector2 dir, float spd)
//     {
//         damage = dmg;
//         direction = dir;
//         speed = spd;
//         pool = FindObjectOfType<ObjectPool>();
//     }

//     private void FixedUpdate()
//     {
//         transform.Translate(direction * speed * Time.fixedDeltaTime, Space.World);
//         if (Mathf.Abs(transform.position.x) > 10f || Mathf.Abs(transform.position.y) > 10f)
//         {
//             pool.Return(gameObject);
//         }
//     }

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             GameManager.Instance.TakePlayerDamage(damage);
//             pool.Return(gameObject);
//         }
//     }
// }