using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Button : MonoBehaviour
{
    public string FunctionName;
    public MonoBehaviour functionHost;
    public void OnInteract()
    {
        functionHost.SendMessage(FunctionName);
    }
}
