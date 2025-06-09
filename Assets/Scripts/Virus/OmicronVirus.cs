using UnityEngine;

public class OmicronVirus : VirusBase
{
    private enum State { Moving, Dodging }
    private State currentState = State.Moving;
    private float dodgeTimer;
    private Vector2 dodgeDirection;

    public override void Initialize(int health, int reward, float spd, Transform tgt, ObjectPool objectPool)
    {
        base.Initialize(health, reward, spd, tgt, objectPool);
        dodgeTimer = 0f;
    }

    protected override void Update()
    {
        base.Update();
        CheckForBullets();
        UpdateState();
    }

    protected override void Move()
    {
        if (target == null) return;

        if (currentState == State.Moving)
        {
            float zigzag = Mathf.Sin(Time.time * 5f) * 0.5f;
            Vector2 direction = (target.position - transform.position).normalized;
            direction += new Vector2(zigzag, 0);
            transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
        }
        else if (currentState == State.Dodging)
        {
            transform.position += (Vector3)(dodgeDirection * speed * 2 * Time.deltaTime);
        }
    }

    private void CheckForBullets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Bullet"))
            {
                currentState = State.Dodging;
                dodgeTimer = 0.5f;
                dodgeDirection = Vector2.Perpendicular((hit.transform.position - transform.position).normalized);
                break;
            }
        }
    }

    private void UpdateState()
    {
        if (currentState == State.Dodging)
        {
            dodgeTimer -= Time.deltaTime;
            if (dodgeTimer <= 0)
            {
                currentState = State.Moving;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}