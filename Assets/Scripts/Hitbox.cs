using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private Collider[] col => GetComponents<Collider>();
    public bool friendly = false;
    public float damage;
    public void EnableHitbox()
    {
        foreach (Collider coll in col)
        {
            coll.enabled = true;
        }
    }

    public void DisableHitbox()
    {
        foreach (Collider coll in col)
        {
            coll.enabled = false;
        }
    }
}
