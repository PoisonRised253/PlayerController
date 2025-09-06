using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player Instance;
    [SerializeField]
    private float maxHealth, step, jumpForce, maxSpeed;
    private float Health;
    private Vector3 pos => transform.position;
    private Vector3 vel => rb.velocity;
    private static Vector3 spos;
    private static Quaternion srot;
    private static Rigidbody rb => Instance.GetComponent<Rigidbody>();
    private static CapsuleCollider col => Instance.GetComponent<CapsuleCollider>();
    [SerializeField]
    public Transform cam,WeaponHelper;
    private Vector3 Forward => Quaternion.Euler(0, -90, 0) * Instance.cam.transform.right;
    private bool crouching, grounded = false;
    [HideInInspector]
    public bool[] allowMove = new bool[3];
    //Assign Keys to Use Here
    private bool w => Input.GetKey(KeyCode.W);
    private bool a => Input.GetKey(KeyCode.A);
    private bool s => Input.GetKey(KeyCode.S);
    private bool d => Input.GetKey(KeyCode.D);
    private bool Dev => Application.isEditor;
    public bool hasSword, hasGrapple, hasDash;
    public GameObject Sword, Grapple;
    public GameObject ItemPool;
    private ConstantForce fallForce;
    private float count;
    void Start()
    {
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        allowMove[0] = true;
        allowMove[1] = true;
        allowMove[2] = true;
        spos = transform.position;
        srot = transform.rotation;
        Health = maxHealth;
        fallForce = Player.Instance.AddComponent<ConstantForce>();
        Application.targetFrameRate = 120;
        StartCoroutine("Fall");
        StartCoroutine("FpsCount");
    }
    void Update()
    {
        Look();
        if (hasSword && Input.GetKeyDown(KeyCode.F))
        {
            Sword.GetComponent<Sword>().StartCoroutine("Yeet");
        }
    }

    private IEnumerator FpsCount()
    {
        GUI.depth = 2;
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(0.1f);
        }
    }
    void FixedUpdate()
    {
        Look();
        if (allowMove[0])
        {
            //Detect Input, Do Movements
            DoMove(PickMove());
        }
        else if (!allowMove[0])
        {
            rb.velocity = Vector3.zero;
        }
        if (allowMove[1] && grounded)
        {
            //Jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.velocity = new Vector3(vel.x, jumpForce, vel.z);
                grounded = false;
            }
        }
        if (allowMove[2])
        {
            //Crouch
            if (Input.GetKeyDown(KeyCode.LeftControl) && !crouching)
            {
                transform.localScale /= 2;
                col.radius /= 2;
                step /= 2;
                allowMove[1] = false;
                crouching = true;
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) && crouching)
            {
                transform.localScale *= 2;
                col.radius *= 2;
                step *= 2;
                allowMove[1] = true;
                crouching = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && hasDash)
        {
            Dash(PickMove());
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Global.Load(SceneManager.GetActiveScene().name);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
        if (hasSword && Input.GetMouseButtonDown(0))
        {
            Sword.GetComponent<Sword>().StartCoroutine("Stab");
        }
        if (hasSword && Input.GetMouseButtonDown(1))
        {
            Sword.GetComponent<Sword>().StartCoroutine("Swing");
        }
        Sword.SetActive(hasSword);

        if (grounded)
        {
            fallTime = 0;
        }
        //Grapple.SetActive(hasGrapple);
        ToggleWeight();
    }

    void LateUpdate()
    {
        LimitSpeed();
    }

    private int PickMove()
    {
        int x = 5;
        if ((w && a && s && d) || (!w && !a && !s && !d) || (w && s) || (a && d))
        {
            //These Conditions lead to no movement
            return 5;
        }
        else
        {
            if (w && a && !s && !d)
            {
                return 7;
            }
            else if (w && !a && !s && d)
            {
                return 9;
            }
            else if (!w && a && s && !d)
            {
                return 1;
            }
            else if (!w && !a && s && d)
            {
                return 3;
            }

            if (w && !a && !s && !d)
            {
                x = 8;
            }
            if (!w && a && !s && !d)
            {
                x = 4;
            }
            if (!w && !a && s && !d)
            {
                x = 2;
            }
            if (!w && !a && !s && d)
            {
                x = 6;
            }
        }

        return x;
    }

    private void DoMove(int input)
    {
        //Convert intager to Direction based on numpad number positions
        switch (input)
        {
            case 5: break;
            case 8: rb.velocity = vel + (Forward * step); break;
            case 2: rb.velocity = vel - (Forward * step); break;
            case 4: rb.velocity = vel - (cam.right * step); break;
            case 6: rb.velocity = vel + (cam.right * step); break;

            case 7: rb.velocity = vel + ((-cam.right + Forward) * step); break;
            case 9: rb.velocity = vel + ((cam.right + Forward) * step); break;
            case 1: rb.velocity = vel + ((-cam.right + -Forward) * step); break;
            case 3: rb.velocity = vel + ((cam.right + -Forward) * step); break;
        }
    }
    void OnCollisionStay(Collision coll)
    {
        if (coll.transform.root.tag == "Map")
        {
            GroundCheck();
        }
    }

    void OnCollisionExit(Collision coll)
    {
        if (coll.transform.root.tag == "Map")
        {
            grounded = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            Item item;
            try
            {
                if (other.transform.parent.TryGetComponent<Item>(out item))
                {
                    if (item.itemName == "Sword")
                    {
                        if (!hasSword)
                        {
                            Inventory.Add("Sword");
                            Destroy(item.gameObject);
                            return;
                        }
                    }
                    else if (item.itemName == "GrappleGun")
                    {
                        Inventory.Add("GrappleGun");
                        Destroy(item.gameObject);
                        return;
                    }
                }
                else return;
            }
            catch (NullReferenceException e)
            {
                new IgnoreException(e.Message);
            }
        }
        Hitbox hitbox;
        if (other.TryGetComponent<Hitbox>(out hitbox))
        {
            StartCoroutine("Damage", hitbox);
        }
    }

    private void GroundCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, transform.localScale.y * 1.5f))
        {
            if (IsFloor(hit.normal))
            {
                hasDash = true;
                grounded = true;
            }
        }
    }
    private void LimitSpeed()
    {
        bool x = false;
        bool z = false;
        bool zp = true;
        bool xp = true;
        Vector3 directionalLimit = new Vector3();

        if (vel.x > maxSpeed)
        {
            x = true;
            xp = true;
        }
        else if (vel.x < -maxSpeed)
        {
            x = true;
            xp = false;
        }

        if (vel.z > maxSpeed)
        {
            z = true;
            zp = true;
        }
        else if (vel.z < -maxSpeed)
        {
            z = true;
            zp = false;
        }

        if (x && z)
        {
            if (xp && zp)
            {
                directionalLimit = new Vector3(maxSpeed, vel.y, maxSpeed);
            }
            else if (xp && !zp)
            {
                directionalLimit = new Vector3(maxSpeed, vel.y, -maxSpeed);
            }
            else if (!xp && zp)
            {
                directionalLimit = new Vector3(-maxSpeed, vel.y, maxSpeed);
            }
            else if (!xp && !zp)
            {
                directionalLimit = new Vector3(-maxSpeed, vel.y, -maxSpeed);
            }
        }
        else if (x && !z)
        {
            if (xp)
            {
                directionalLimit = new Vector3(maxSpeed, vel.y, vel.z);
            }
            else if (!xp)
            {
                directionalLimit = new Vector3(-maxSpeed, vel.y, vel.z);
            }
        }
        else if (!x && z)
        {
            if (zp)
            {
                directionalLimit = new Vector3(vel.x, vel.y, maxSpeed);
            }
            else if (!zp)
            {
                directionalLimit = new Vector3(vel.x, vel.y, -maxSpeed);
            }
        }

        if (x || z)
        {
            rb.velocity = directionalLimit;
        }
    }

    private void Interact()
    {
        RaycastHit hit;
        Button button;
        if (Physics.Raycast(cam.position, cam.forward, out hit, 2f, LayerMask.GetMask("Interact")))
        {
            if(hit.transform.TryGetComponent<Button>(out button))
            {
                button.OnInteract();
            }
        }
    }

    private void Dash(int input)
    {
        float Modifier = 25f + Mathf.Abs(+vel.x + +vel.y + +vel.z) / 3;
        switch (input)
        {
            case 5: break;
            case 8: rb.AddForce(cam.forward * Modifier, ForceMode.Impulse); break;
            case 2: rb.AddForce(-cam.forward * Modifier, ForceMode.Impulse); break;
            case 4: rb.AddForce(-cam.right * Modifier, ForceMode.Impulse); break;
            case 6: rb.AddForce(cam.right * Modifier, ForceMode.Impulse); break;

            case 7: rb.AddForce((cam.forward + -cam.right) * Modifier, ForceMode.Impulse); break;
            case 9: rb.AddForce((cam.forward + cam.right) * Modifier, ForceMode.Impulse); break;
            case 1: rb.AddForce((-cam.forward + -cam.right) * Modifier, ForceMode.Impulse); break;
            case 3: rb.AddForce((-cam.forward + cam.right) * Modifier, ForceMode.Impulse); break;
        }
        
        hasDash = false;
    }
    private bool isFalling => !grounded;
    public int fallTime = 0;
    private IEnumerator Fall()
    {
        if (!grounded)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            if (!grounded)
            {
                fallTime = fallTime + 1;
                fallForce.relativeForce = new Vector3(0,-fallTime * 50,0);
            }
        }
        if (grounded)
        {
            fallTime = 0;
        }
        yield return new WaitForSecondsRealtime(0.5f);
        StartCoroutine("Fall");
    }

    private IEnumerator Damage(Hitbox box)
    {
        Health -= box.damage;
        if (Health < 0f)
        {
            print("Death");
            Restart();
            StopCoroutine("Damage");
        }
        else if (Health > 0f)
        {
            print("Pain");
            Global.Instance.effects.StartCoroutine("OnHit");
            StopCoroutine("Damage");
        }
        yield return new WaitForSeconds(1f);
    }
    private bool ToggleWeight()
    {
        bool x = false;
        if (fallTime > 0)
        {
            fallForce.enabled = true;
            x = true;
        }
        else if (fallTime <= 0)
        {
            x = false;
            fallForce.enabled = false;
        }
        return x;
    }

    public static void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(5, 40, 100, 25), "Health: " + Health);
        GUI.Label(new Rect(5, 60, 100, 25), "FPS: " + Mathf.Round(count));
    }

    //Anything below: Stolen from DaniDev
    private float desiredX;
    private float xRotation;
    private readonly float sensitivity = 100;
    private readonly float sensMultiplier = 1;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        Vector3 rot = cam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
    }
    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < 75f;
    }
}