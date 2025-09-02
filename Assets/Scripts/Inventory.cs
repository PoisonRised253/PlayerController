using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private static GameObject[] GivenItems;
    public static GameObject[] Items;
    public static void Add(string type)
    {
        if (type == "Sword")
        {
            Player.Instance.hasSword = true;
        }
        if (type == "GrappleGun")
        {
            Player.Instance.hasGrapple = true;
        }
    }
    public static void Remove(string type)
    {
        if (type == "Sword")
        {
            Player.Instance.hasSword = false;
        }
        if (type == "GrappleGun")
        {
            Player.Instance.hasGrapple = false;
        }
    }
    public static void Reset()
    {
        Items = GivenItems;
    }
}
