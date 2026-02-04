using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

public class ShadowController : MonoBehaviour
{
    private SpriteRenderer sp;
    public GameObject target;

    private float elapsed = 0;
    public float time = 1f;
    void Start()
    {
        sp = GetComponent<SpriteRenderer>();

        transform.localScale = target.transform.localScale;
        sp.sprite = target.GetComponent<SpriteRenderer>().sprite;
        sp.color = target.GetComponent<SpriteRenderer>().color;
    }

    
    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / time)*0.5f;
        sp.color = new Color(sp.color.r,sp.color.g,sp.color.b,0.5f-t);

        if(elapsed>=time)Destroy(gameObject);
    }
}
