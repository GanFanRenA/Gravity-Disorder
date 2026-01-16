using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Vector2 personalGravity = new Vector2(-5,0); 
    public InputManager inputs;
    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private AudioSource audioSource;

    public AudioClip switchSound;
    public AudioClip modeSound;
    public AudioClip collideSound;
    public AudioClip deadSound;

    public float speed = 5f;
    private Vector2 lastVelocity;
    public bool isNoGravity = true;
    public bool isLanding = false;

    private void Awake() {
        rb=GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        inputs = new InputManager();
        inputs.Enable();
        isNoGravity = true;

        inputs.Player.PressMove.started += ChangeGravity;
        inputs.Player.SwitchMode.started += SwitchMode;
        inputs.Player.QuitGame.started += QuitGame;
    }

    private void QuitGame(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Application.Quit();
    }

    private void SwitchMode(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        isNoGravity = !isNoGravity;
        if(isNoGravity)sp.color = Color.white;
        else sp.color = Color.black;
        audioSource.PlayOneShot(modeSound);
    }


    void FixedUpdate()
    {
        rb.AddForce(personalGravity,ForceMode2D.Force);
        if(!isLanding)ChangeScale();
    }

    void LateUpdate()
    {
        lastVelocity = rb.velocity;
    }

    private void ChangeGravity(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(isLanding)return;

        audioSource.PlayOneShot(switchSound);
        if(isNoGravity)rb.velocity = new Vector2(0,0);
        personalGravity = inputs.Player.Move.ReadValue<Vector2>() * speed;
    }

    private void LandEffect()
    {
        float xVelocityChange = Mathf.Abs(rb.velocity.x - lastVelocity.x);
        float yVelocityChange = Mathf.Abs(rb.velocity.y - lastVelocity.y);
        
        
        if (xVelocityChange > 1f && xVelocityChange>yVelocityChange)
        {
            // 根据速度计算压缩程度：速度越大，X轴压缩越少（保持宽度），Y轴压缩越多（变扁）
            float compressionFactor = Mathf.Sqrt(Mathf.Clamp01(xVelocityChange / 60f));
            Vector3 targetScale = new Vector3(
                1f - compressionFactor * 0.5f,
                1f + compressionFactor * 1f,
                1
            );
            StartCoroutine(RunLandEffect(targetScale));
        }
        
        if (yVelocityChange > 1f && xVelocityChange<yVelocityChange)
        {
            // 根据速度计算压缩程度：速度越大，X轴压缩越多（变宽），Y轴压缩越多（变扁）
            float compressionFactor = Mathf.Sqrt(Mathf.Clamp01(yVelocityChange / 60f));
            Vector3 targetScale = new Vector3(
                1f + compressionFactor * 1.0f,
                1f - compressionFactor * 0.5f,
                1
            );
            StartCoroutine(RunLandEffect(targetScale));
        }
    }

    IEnumerator RunLandEffect(Vector3 targetVec)
    {
        isLanding = true;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.1f);
            
            // 线性插值
            transform.localScale = Vector3.Lerp(startScale, targetVec, t);
            
            yield return null;
        }

        isLanding = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        audioSource.PlayOneShot(collideSound);
        LandEffect();
    }

    void ChangeScale()
    {
        if(Mathf.Abs(transform.rotation.eulerAngles.z-90)<1f || Mathf.Abs(transform.rotation.eulerAngles.z-270)<1f){
            transform.rotation = Quaternion.Euler(0,0,0);
        }

        Vector2 velocity = rb.velocity;
        
        // Get the overall speed and direction
        float speed = velocity.magnitude;
        Vector2 direction = velocity.normalized;
        
        // Calculate scaling factor based on speed
        float factor = Mathf.Clamp(speed * 0.02f, 0f, 0.2f);
        
        // Stretch in movement direction, compress perpendicular
        float targetScaleX = 1 + factor * Mathf.Abs(direction.x);
        float targetScaleY = 1 + factor * Mathf.Abs(direction.y);
        
        // Add compression in opposite axis
        targetScaleX -= factor * Mathf.Abs(direction.y) * 0.5f;
        targetScaleY -= factor * Mathf.Abs(direction.x) * 0.5f;
        
        // Ensure minimum scale
        targetScaleX = Mathf.Max(targetScaleX, 0.8f);
        targetScaleY = Mathf.Max(targetScaleY, 0.8f);
        
        Vector3 targetScale = new Vector3(targetScaleX, targetScaleY, 1);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10);
    }
}