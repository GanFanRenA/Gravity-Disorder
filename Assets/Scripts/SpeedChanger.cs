using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedChanger : MonoBehaviour
{
    [SerializeField]private float speedMultiplier = 1f;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>().personalGravity *=speedMultiplier;
            collision.gameObject.GetComponent<PlayerController>().maxSpeed *=speedMultiplier;
        }
    }
}
