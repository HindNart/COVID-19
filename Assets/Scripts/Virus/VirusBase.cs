using UnityEngine;

public abstract class VirusBase : MonoBehaviour, IDamageable
{
    protected int hp;
    protected int points;
    protected float speed;
    protected Transform target;
    protected ObjectPool pool;

    public virtual void Initialize(int health, int reward, float spd, Transform tgt, ObjectPool objectPool)
    {
        hp = health;
        points = reward;
        speed = spd;
        target = tgt;
        pool = objectPool;
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
        GameManager.Instance.AddAntibodies(points);
        pool.Return(gameObject);
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.TakePlayerDamage(10);
            pool.Return(gameObject);
        }
    }
}