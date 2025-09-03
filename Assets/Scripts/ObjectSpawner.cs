using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField]
    public bool shouldSpawn = false;
    public bool isSpawning { get; private set; }
    public bool setRotation = false;
    public bool setVelocity = false;
    [SerializeField]
    private float spawnInterval = 3f;
    [SerializeField]
    public GameObject ObjectToSpawn;
    [SerializeField]
    private Quaternion Rotation;
    [SerializeField]
    private Vector3 velocity;
    private Quaternion SpawnRotation()
    {
        if (setRotation)
        {
            return Rotation;
        }
        else return Random.rotation;
    }
    private Vector3 SpawnVelocity()
    {
        if (setVelocity)
        {
            return velocity;
        }
        else return new Vector3();
    }

    public IEnumerator Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
        if (shouldSpawn)
        {
            var x = Instantiate(ObjectToSpawn, position: transform.position, SpawnRotation(), transform);
            isSpawning = true;
            NavMeshAgent agent;
            x.TryGetComponent<NavMeshAgent>(out agent);
            agent.enabled = true;
            if (setVelocity)
            {
                Velocity(x.transform);
            }
        }
        else
        {
            isSpawning = false;
        }


        yield return new WaitForSecondsRealtime(spawnInterval);
        StartCoroutine("Start");
    }

    public void SpawnOnce()
    {
        NavMeshAgent agent;
        var x = Instantiate(ObjectToSpawn, position: transform.position, SpawnRotation(), transform);
        x.TryGetComponent<NavMeshAgent>(out agent);
        agent.enabled = true;
        if (setVelocity)
        {
            Velocity(x.transform);
        }
    }

    public void ToggleSpawning()
    {
        shouldSpawn = !shouldSpawn;
    }

    public void Remove()
    {
        List<Transform> children = new();
        for (var i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }

        foreach (Transform tr in children)
        {
            Destroy(tr.gameObject);
        }
    }

    private void Velocity(Transform item)
    {
        item.GetComponent<Rigidbody>().velocity = velocity;
    }
}
