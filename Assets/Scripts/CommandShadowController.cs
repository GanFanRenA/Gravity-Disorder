using UnityEngine;
using UnityEngine.AI;

public class CommandShadowController : MonoBehaviour
{
    private bool isShown = false;
    private SpriteRenderer sp;
    public GameObject target;
    [SerializeField]private GameObject sign;
    void Start()
    {
        sp = GetComponent<SpriteRenderer>();

        transform.localScale = target.transform.localScale;
        sp.sprite = target.GetComponent<SpriteRenderer>().sprite;
        sp.color = target.GetComponent<SpriteRenderer>().color;
        sp.color = new Color(sp.color.r,sp.color.g,sp.color.b,0);
        
        Vector2 vec = target.GetComponent<PlayerController>().personalGravity;
        if (vec.x != 0)
        {
            if(vec.x>0)sign.transform.localRotation = Quaternion.Euler(0,0,90);
            else sign.transform.localRotation = Quaternion.Euler(0,0,-90);
        }
        else
        {
            if(vec.y>0)sign.transform.localRotation = Quaternion.Euler(0,0,0);
            else sign.transform.localRotation = Quaternion.Euler(0,0,180);
        }

        
    }

    public void Shown()
    {
        if(isShown)Destroy(gameObject);
        sp.color = new Color(sp.color.r,sp.color.g,sp.color.b,0.4f);
        isShown=true;
        
    }
}
