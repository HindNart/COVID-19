using System.Collections;
using UnityEngine;

public abstract class VirusBase : MonoBehaviour, IDamageable
{
    [SerializeField] protected GameObject deathEffectPrefab;
    [SerializeField] protected GameObject trailEffectPrefab;

    protected int hp;
    protected float points;
    protected float speed;
    protected Transform target;
    protected ObjectPool pool;
    private ParticleSystem trailEffect;

    public virtual void Initialize(int health, float reward, float spd, Transform tgt, ObjectPool objectPool)
    {
        hp = health;
        points = reward;
        speed = spd;
        target = tgt;
        pool = objectPool;

        // Khởi tạo Particle System vệt di chuyển
        if (trailEffectPrefab != null)
        {
            GameObject trailObj = pool.Get(trailEffectPrefab);
            trailObj.transform.SetParent(transform);
            trailObj.transform.localPosition = Vector3.zero;
            trailEffect = trailObj.GetComponent<ParticleSystem>();
            trailEffect.Play();
        }
    }

    protected virtual void Update()
    {
        Move();
    }

    protected abstract void Move();

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("VirusDeath");
        }

        // Phát Particle System hiệu ứng chết
        if (deathEffectPrefab != null && pool != null)
        {
            GameObject effect = pool.Get(deathEffectPrefab);
            if (effect != null)
            {
                effect.transform.position = transform.position;
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                ps.Play();
                // Chạy Coroutine trên ObjectPool để tránh gián đoạn
                pool.StartCoroutine(ReturnEffectToPool(effect, ps.main.duration));
            }
        }

        // Trả Particle System vệt di chuyển về pool
        if (trailEffect != null)
        {
            trailEffect.Stop();
            pool.Return(trailEffect.gameObject);
        }

        pool.Return(gameObject);

        GameManager.Instance.AddAntibodies(points);
        if (Random.value < 0.05f) // 5% drop power-up
        {
            GameManager.Instance.SpawnPowerUp(transform.position);
        }
    }

    protected IEnumerator ReturnEffectToPool(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Return(effect);
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.TakePlayerDamage(5);
            pool.Return(gameObject);
        }
    }
}