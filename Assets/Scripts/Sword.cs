using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public GameObject PhysSword;
    Animation anim => GetComponent<Animation>();
    public bool ready;
    private Quaternion restRot;
    [SerializeField]
    public float damage = 1.0f;

    void Start()
    {
        Reset(true);
    }
    public IEnumerator Yeet()
    {
        if (Player.Instance.hasSword)
        {
            var x = Instantiate(PhysSword, position: Player.Instance.transform.position + Player.Instance.transform.forward, new());
            yield return new WaitForSeconds(0.25f);
            x.GetComponent<Rigidbody>().AddForce(Player.Instance.cam.forward * 100f);
            Player.Instance.hasSword = false;
        }
    }
    public IEnumerator Stab()
    {
        if (ready)
        {
            ready = false;
            anim.Play("Stab", PlayMode.StopAll);
            yield return new WaitForSeconds(anim.GetClip("Stab").length);
            ready = true;
        }
        yield return new WaitForSeconds(0.0002f);
        Reset(false);
    }
    public IEnumerator Swing()
    {
        if (ready)
        {
            ready = false;
            anim.Play("Swing", PlayMode.StopAll);
            yield return new WaitForSeconds(anim.GetClip("Swing").length);
            ready = true;
        }
        yield return new WaitForSeconds(0.0002f);
        Reset(false);
    }

    private void Reset(bool a)
    {
        if (a)
        {
            //Save
            restRot = transform.localRotation;
            return;
        }
        if (!a)
        {
            //Load
            transform.localRotation = restRot;
            return;
        }
    }
}
