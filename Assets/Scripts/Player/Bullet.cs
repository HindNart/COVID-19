using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    private Vector2 direction;
    private float speed = 10f;
    private ObjectPool pool;

    public void Initialize(int dmg, Vector2 dir)
    {
        damage = dmg;
        direction = dir;
        pool = FindObjectOfType<ObjectPool>();
    }

    private void FixedUpdate()
    {
        transform.Translate(direction * speed * Time.fixedDeltaTime);
        if (Mathf.Abs(transform.position.x) > 10f || Mathf.Abs(transform.position.y) > 10f)
        {
            pool.Return(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Virus"))
        {
            other.GetComponent<IDamageable>().TakeDamage(damage);
            pool.Return(gameObject);
        }
    }
}