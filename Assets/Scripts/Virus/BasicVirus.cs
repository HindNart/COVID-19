using UnityEngine;

public class BasicVirus : VirusBase
{
    protected override void Move()
    {
        if (target == null)
        {
            // Cập nhật target liên tục bằng cách tìm Player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                return;
            }
        }
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }
}