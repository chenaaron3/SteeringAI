using UnityEngine;
using System.Collections;

public class Flag : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController pc = collision.GetComponent<PlayerController>();
            pc.lap++;
            if (pc.lap == 3)
            {
                pc.Die();
            }
        }
    }
}
