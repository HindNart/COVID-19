using System.Collections;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Heal, SpeedBoost, Shield, Explosion }
    [SerializeField] private PowerUpType type;
    [SerializeField] private float duration = 10f;
    [SerializeField] private GameObject explosionEffectPrefab;
    private ObjectPool pool;
    private Collider2D col;

    private void Awake()
    {
        pool = FindObjectOfType<ObjectPool>();
        col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }
    }

    private void FixedUpdate()
    {
        if (Mathf.Abs(transform.position.x) > 11.5f || Mathf.Abs(transform.position.y) > 9f)
        {
            pool.Return(gameObject);
        }
    }

    public void TryActivate(GameObject playerObj)
    {
        if (playerObj != null)
        {
            ApplyEffect(playerObj);
            pool.Return(gameObject);
        }
    }

    private void ApplyEffect(GameObject player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null) return;

        switch (type)
        {
            case PowerUpType.Heal:
                GameManager.Instance.HealPlayer(20);
                break;
            case PowerUpType.SpeedBoost:
                playerController.ApplySpeedBoost(2f, duration);
                break;
            case PowerUpType.Shield:
                playerController.ApplyShield(duration);
                break;
            case PowerUpType.Explosion:
                GameManager.Instance.TriggerExplosion();
                if (explosionEffectPrefab != null)
                {
                    GameObject effect = pool.Get(explosionEffectPrefab);
                    effect.transform.position = Vector2.zero; // Nổ tại trung tâm
                    ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                    ps.Play();
                    // Chạy Coroutine trên ObjectPool để tránh gián đoạn
                    pool.StartCoroutine(ReturnEffectToPool(effect, ps.main.duration));
                }
                break;
        }
    }

    private IEnumerator ReturnEffectToPool(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Return(effect);
    }
}