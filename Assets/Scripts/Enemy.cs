using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    NavMeshAgent agent => GetComponent<NavMeshAgent>();
    public Animation anim;
    public Transform Target, HitBoxObject, PhysModel;
    public float IterationTimer = 0.5f;
    private bool wait = true;
    public bool chase = true;
    public float MaxHealth, Health = 3f;
    public float range = 5f;
    // Start is called before the first frame update
    void Start()
    {
        Health = MaxHealth;
        StartCoroutine("EvalGoal");
        anim = transform.GetChild(0).GetComponent<Animation>();
        StartCoroutine("Animation");
    }

    private IEnumerator EvalGoal()
    {
        if (wait)
        {
            yield return new WaitForSeconds(0.5f);
            wait = false;
        }
        if (chase && isActive())
        {
            agent.destination = Target.position;
        }
        else if (isActive())
        {
            agent.destination = agent.transform.position;
        }
        yield return new WaitForSeconds(IterationTimer);
        StartCoroutine("EvalGoal");
    }

    public void Death()
    {
        Destroy(this.gameObject);
    }
    void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.root == Player.Instance.transform && !Player.Instance.Sword.GetComponent<Sword>().ready)
        {
            Damage();
        }
    }

    private void Damage()
    {
        var damage = Player.Instance.Sword.GetComponent<Sword>().damage;
        if (Health < damage)
        {
            Instantiate(PhysModel, position: transform.position, rotation: Random.rotation, null);
            Death();
        }
        else
        {
            Health = Health - damage;
        }
    }

    private IEnumerator Animation()
    {
        if (InRange())
        {
            anim.Blend("RobotSwing", 1f);
            yield return new WaitForSeconds(0.2f * Time.deltaTime);
            EnableHitbox();
            yield return new WaitForSeconds(0.25f * Time.deltaTime);
            DisableHitbox();
        }
        else if (chase)
        {
            anim.Blend("RobotMove", 1f);
            yield return new WaitForSeconds(anim.GetClip("RobotMove").length * Time.deltaTime);
        }
        yield return new WaitForSeconds(0.5f);
        StartCoroutine("Animation");
    }

    private bool InRange()
    {
        return Vector3.Distance(Player.Instance.transform.position, transform.position) < range;
    }

    public void EnableHitbox()
    {
        HitBoxObject.GetComponent<Hitbox>().EnableHitbox();
    }

    public void DisableHitbox()
    {
        HitBoxObject.GetComponent<Hitbox>().DisableHitbox();
    }

    public bool isActive()
    {
        return agent.enabled;
    }
}
