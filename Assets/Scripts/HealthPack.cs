using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{

    void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            if (!player.OnObjectCollision(true))
                return;

            player.SetLives(player.lives + 1);

            Destroy(gameObject);
        }
        else if (other.CompareTag("Border"))
        {
            Destroy(gameObject);
        }
    }
}
