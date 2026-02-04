using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleSpriteController : MonoBehaviour
{
    public SpriteRenderer sp;
    public ParticleSystem ps;
    public Vector3 targetScale;
    public Vector3 basicScale;
    public float time;
    private float elapsed = 0f;
    void Awake()
    {
        basicScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / time);
        transform.localScale = Vector3.Lerp(basicScale, targetScale, t);

        if(elapsed>=time)Destroy(gameObject);
    }
}
