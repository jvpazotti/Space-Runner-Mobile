using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenCylinder : MonoBehaviour
{
    public float oxygenValue = 20f;

    void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            if (!player.OnObjectCollision())
                return;

            player.ReplenishOxygen(oxygenValue);

            Destroy(gameObject);
        }
        else if (other.CompareTag("Border"))
        {
            Destroy(gameObject);
        }
    }
}
