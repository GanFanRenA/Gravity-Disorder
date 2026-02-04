using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

public class LogicManager : MonoBehaviour
{
    public static LogicManager Instance;
    [SerializeField]private List<BreakablePlatformController> allBlocks = new List<BreakablePlatformController>();
    [SerializeField]private List<CommandShadowController> allCommandShadow = new List<CommandShadowController>();
    
    void Start()
    {
        Instance = this;
        FindAllBlocks();
    }
    
    void FindAllBlocks()
    {
        allBlocks.Clear();
        BreakablePlatformController[] blocks = FindObjectsOfType<BreakablePlatformController>();
        allBlocks.AddRange(blocks);
    }
    void FindAllShadow()
    {
        allCommandShadow.Clear();
        CommandShadowController[] blocks = FindObjectsOfType<CommandShadowController>();
        allCommandShadow.AddRange(blocks);
    }
    
    public void OnPlayerDeath()
    {
        FindAllShadow();
        foreach (BreakablePlatformController block in allBlocks)
        {
            if (block != null)
            {
                if(block.gameObject.activeSelf)block.gameObject.GetComponent<BreakablePlatformController>().Reset();
                else block.gameObject.SetActive(true);
            }
        }
        foreach (CommandShadowController block in allCommandShadow)
        {
            if (block != null)
            {
                block.Shown();
            }
        }
    }
}
