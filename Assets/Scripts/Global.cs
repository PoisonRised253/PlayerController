using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Global : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("User Exit!");
        Application.Quit();
    }
    public static void Load(string mapToPass)
    {
        SceneManager.LoadScene(mapToPass, LoadSceneMode.Single);
    }

    public static List<Item> GetAllItems()
    {
        var x = new List<Item>();
        for (var i = 0; i < Player.Instance.ItemPool.transform.childCount; i++)
        {
            x.Add(Player.Instance.ItemPool.transform.GetChild(i).GetComponent<Item>());
        }
        return x;
    }
}
