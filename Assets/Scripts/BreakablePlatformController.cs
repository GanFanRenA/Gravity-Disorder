using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakablePlatformController : MonoBehaviour
{
    [SerializeField]private float health = 3;
    private Animator anim;
    private float currentHealth = 0;
    [SerializeField]private bool isTimed = false;
    private bool isTouch = false;

    private float nextPercent = 0.8f;

    void Start()
    {
        anim = GetComponent<Animator>();
    }
    void OnEnable()
    {
        Reset();
    }

    public void Reset(){
        currentHealth = health;
        anim.SetTrigger("Reset");
        nextPercent = 0.8f;
        isTouch = false;
    }

    void Update()
    {
        if(currentHealth/health < nextPercent)
        {
            nextPercent-=0.2f;
            anim.SetTrigger("ChangePhase");
        }

        if(isTouch&&isTimed)currentHealth-=Time.deltaTime;
        if(currentHealth<=0)gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            currentHealth--;
            isTouch=true;
            if(currentHealth<=0)gameObject.SetActive(false);
        }
    }
}
