using DG.Tweening;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject trailEffectPrefab;
    private int damage;
    private Vector2 direction;
    private float speed = 10f;
    private ObjectPool pool;
    private ParticleSystem trailEffect;

    public void Initialize(int dmg, Vector2 dir)
    {
        damage = dmg;
        direction = dir;
        pool = FindObjectOfType<ObjectPool>();

        // Phát Particle System
        if (trailEffectPrefab != null)
        {
            GameObject trailObj = pool.Get(trailEffectPrefab);
            trailObj.transform.SetParent(transform);
            trailObj.transform.localPosition = Vector3.zero + new Vector3(0, -1, 0);
            trailEffect = trailObj.GetComponent<ParticleSystem>();
            trailEffect.Play();
        }
    }

    private void FixedUpdate()
    {
        // transform.Translate(direction * speed * Time.fixedDeltaTime, Space.World);
        transform.DOMove(transform.position + (Vector3)direction * speed * Time.fixedDeltaTime, 0.01f).SetEase(Ease.Linear);
        // Kiểm tra nếu đạn ra khỏi
        if (Mathf.Abs(transform.position.x) > 11.5f || Mathf.Abs(transform.position.y) > 9f)
        {
            // Trả Particle System vệt đạn về pool
            if (trailEffect != null)
            {
                trailEffect.Stop();
                pool.Return(trailEffect.gameObject);
            }
            pool.Return(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Virus"))
        {
            other.GetComponent<IDamageable>().TakeDamage(damage);
            // Trả Particle System vệt đạn về pool
            if (trailEffect != null)
            {
                trailEffect.Stop();
                pool.Return(trailEffect.gameObject);
            }
            pool.Return(gameObject);
        }
    }
}