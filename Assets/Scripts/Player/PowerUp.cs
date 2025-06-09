using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Heal, SpeedBoost, Shield, Explosion }
    [SerializeField] private PowerUpType type;
    [SerializeField] private float duration = 10f;
    private ObjectPool pool;

    private void Awake()
    {
        pool = FindObjectOfType<ObjectPool>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyEffect(other.gameObject);
            pool.Return(gameObject);
        }
    }

    private void ApplyEffect(GameObject player)
    {
        PlayerController playerScript = player.GetComponent<PlayerController>();
        switch (type)
        {
            case PowerUpType.Heal:
                GameManager.Instance.HealPlayer(20);
                break;
            case PowerUpType.SpeedBoost:
                playerScript.ApplySpeedBoost(2f, duration);
                break;
            case PowerUpType.Shield:
                playerScript.ApplyShield(3, duration);
                break;
            case PowerUpType.Explosion:
                GameManager.Instance.TriggerExplosion();
                break;
        }
    }
}