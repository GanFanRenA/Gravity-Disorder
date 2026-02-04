using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Vector2 personalGravity = new Vector2(-5,0); 
    public InputManager inputs;
    public Rigidbody2D rb;
    private SpriteRenderer sp;
    private AudioSource audioSource;
    public GameObject waveObj;
    public GameObject shadowObj;
    public GameObject gravityParticleObj;
    public GameObject commandShadowObj;

    public AudioClip switchSound;
    public AudioClip modeSound;
    public AudioClip collideSound;
    public AudioClip deadSound;
    public AudioClip checkSound;

    public float speed = 5f;
    private float moveDistance = 0f;
    private float rbMultiplier = 0f;
    private Vector2 lastVelocity;
    public bool isNoGravity = true;
    public bool isLanding = false;
    public Vector2 respawnPoint;

    [Header("速度限制")]
    public float maxSpeed = 10f;

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

    public void SwitchMode(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        isNoGravity = !isNoGravity;
        if(isNoGravity)sp.color = Color.white;
        else sp.color = Color.black;
        audioSource.PlayOneShot(modeSound);

        GameObject obj = Instantiate(waveObj,transform.position,transform.rotation);
        obj.GetComponent<ScaleSpriteController>().sp.color = sp.color;
    }

    void Update()
    {
        moveDistance+=(Mathf.Abs(rb.velocity.x)+Mathf.Abs(rb.velocity.y))*Time.deltaTime;
        if(rbMultiplier>1.5f)rbMultiplier=1;
        else if(rbMultiplier>0)rbMultiplier-=Time.deltaTime*0.5f;
        else rbMultiplier=0;

        if(isNoGravity)sp.color = new Color(1-(rbMultiplier-0.5f),1-(rbMultiplier-0.5f),1-(rbMultiplier-0.5f));

        if(moveDistance>0.5f)
        {
            GameObject obj = Instantiate(shadowObj,transform.position,transform.rotation);
            obj.GetComponent<ShadowController>().target = gameObject;
            moveDistance=0f;
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(personalGravity,ForceMode2D.Force);
        if(!isLanding)ChangeScale();

        Vector2 currentVelocity = rb.velocity;

        currentVelocity.x = Mathf.Clamp(currentVelocity.x, -maxSpeed, maxSpeed);
        currentVelocity.y = Mathf.Clamp(currentVelocity.y, -maxSpeed, maxSpeed);
        
        rb.velocity = currentVelocity;
    }

    void LateUpdate()
    {
        lastVelocity = rb.velocity;
    }

    private void ChangeGravity(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(isLanding)return;

        audioSource.PlayOneShot(switchSound);

        GameObject obj1 = Instantiate(commandShadowObj,transform.position,transform.rotation);
        obj1.GetComponent<CommandShadowController>().target = gameObject;

        personalGravity = inputs.Player.Move.ReadValue<Vector2>() * speed;

        maxSpeed=15f;

        if(isNoGravity){
            rbMultiplier-=0.5f;
            float temp=0;
            if(rbMultiplier<0){
                temp=Mathf.Abs(rbMultiplier);
                rbMultiplier=0;
            }
            rb.velocity = Vector2.zero+new Vector2(rbMultiplier*Random.Range(-10f,10f),rbMultiplier*Random.Range(-10f,10f));
            rbMultiplier += 1f - temp;
        }
        else rb.velocity *= 0.5f;

        GameObject obj2 = Instantiate(gravityParticleObj,transform.position,transform.rotation);
        var main = obj2.GetComponent<ScaleSpriteController>().ps.main;
        main.startColor = sp.color;
    }

    private void LandEffect()
    {
        float xVelocityChange = Mathf.Abs(rb.velocity.x - lastVelocity.x);
        float yVelocityChange = Mathf.Abs(rb.velocity.y - lastVelocity.y);
        
        
        if (xVelocityChange > 1f && xVelocityChange>yVelocityChange)
        {
            // 根据速度计算压缩程度：速度越大，X轴压缩越少（保持宽度），Y轴压缩越多（变扁）
            float compressionFactor = Mathf.Sqrt(Mathf.Clamp01(xVelocityChange / maxSpeed));
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
            float compressionFactor = Mathf.Sqrt(Mathf.Clamp01(yVelocityChange / maxSpeed));
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
            
            transform.localScale = Vector3.Lerp(startScale, targetVec, t);
            
            yield return null;
        }

        isLanding = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.CompareTag("Enemy")&&!isLanding)Death();
        else{
            audioSource.PlayOneShot(collideSound);
            LandEffect();
        }
        
    }

    public void CheckPoint(Vector2 vec)
    {
        audioSource.PlayOneShot(checkSound);

        respawnPoint = vec;
    }

    private void Death()
    {
        LogicManager.Instance.OnPlayerDeath();
        audioSource.PlayOneShot(deadSound);
        if(!isNoGravity)SwitchMode(new UnityEngine.InputSystem.InputAction.CallbackContext());

        GameObject obj1 = Instantiate(gravityParticleObj,transform.position,transform.rotation);
        var main = obj1.GetComponent<ScaleSpriteController>().ps.main;
        main.startColor = sp.color;


        GameObject obj2 = Instantiate(shadowObj,transform.position,transform.rotation);
        obj2.GetComponent<ShadowController>().target = gameObject;

        transform.position = respawnPoint;
        personalGravity = Vector2.zero;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        transform.rotation = Quaternion.identity;
        rbMultiplier=0;
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