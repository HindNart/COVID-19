using UnityEngine;

public class BasicVirus : VirusBase
{
    protected override void Move()
    {
        if (target != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
    }
}