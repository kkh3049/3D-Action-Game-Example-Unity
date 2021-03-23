using UnityEngine;
using System.Collections;
using Mirror;

enum Direction
{
    LEFT,
    RIGHT,
}

public class Valken : NetworkBehaviour
{
    Animator anim;
    Direction dir = Direction.RIGHT;

    public GameObject Bomb;
    public ParticleSystem RightMuzzle, LeftMuzzle, RightFire, LeftFire;
    public Transform LeftArm, RightArm;
    public ParticleSystem Boost;
    public Transform MissilePoint;


    public JoyStick MyStick;
    public GameButton Abutton, Bbutton;

    public Light LeftLight, RightLight;

    public float speed = 4.0f;
    public bool isMoving = false;
    float JumpTimer = 0f;

    public override void OnStartLocalPlayer()
    {
        Camera.main.GetComponent<FollowCam>().TargetPlayer = transform;
    }
    
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        anim.Play("Walk");
        RightMuzzle.emissionRate = RightFire.emissionRate = LeftMuzzle.emissionRate = LeftFire.emissionRate = 0;
    }

    IEnumerator LightControl()
    {
        while (true)
        {
            LeftLight.intensity = RightLight.intensity = 1.0f;
            yield return new WaitForSeconds(0.03f);
            LeftLight.intensity = RightLight.intensity = 0f;
            yield return new WaitForSeconds(0.03f);
        }
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position + transform.forward * 0.4f + transform.up * 0.1f, Vector3.down, 0.1f);
    }

    public GameObject RayGround()
    {
        GameObject temp = null;
        RaycastHit hit;
        Physics.Raycast(transform.position + transform.forward * 0.4f + transform.up * 0.1f, Vector3.down, out hit, 0.1f);
        if (hit.collider != null) temp = hit.collider.gameObject;
        return temp;
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        var horizontal = Input.GetAxis("Horizontal");
        if (horizontal < 0)
        {
            if (!LeanTween.isTweening(gameObject))
            {
                if (isGrounded()) anim.Play("Walk");
                else anim.Play("Idle");

                if (dir != Direction.LEFT)
                {
                    LeanTween.rotateAroundLocal(gameObject, Vector3.up, 180f, 0.3f).setOnComplete(TurnLeft);
                }
                else
                {
                    transform.Translate(Vector3.forward * speed * Time.deltaTime);
                }
                isMoving = true;
            }
        }
        else if (horizontal > 0)
        {
            if (!LeanTween.isTweening(gameObject))
            {
                if (isGrounded()) anim.Play("Walk");
                else anim.Play("Idle");

                if (dir != Direction.RIGHT)
                {
                    LeanTween.rotateAroundLocal(gameObject, Vector3.up, -180f, 0.3f).setOnComplete(TurnRight);
                }
                else
                {
                    transform.Translate(Vector3.forward * speed * Time.deltaTime);
                }
                isMoving = true;
            }
        }
        else
        {
            anim.Play("Idle");
            isMoving = false;
        }

        var vertical = Input.GetAxis("Vertical");

        if (vertical > 0)
        {
            RightArm.Rotate(Vector3.back * 200f * Time.deltaTime);
            LeftArm.Rotate(Vector3.back * 200f * Time.deltaTime);
        }
        else if (vertical < 0)
        {
            RightArm.Rotate(Vector3.forward * 200f * Time.deltaTime);
            LeftArm.Rotate(Vector3.forward * 200f * Time.deltaTime);
        }

        if (MyStick.isClick)
        {
            if (dir == Direction.RIGHT)
            {
                RightArm.localRotation = Quaternion.Euler(new Vector3(0f, 90f, MyStick.Degree - 90f));
                LeftArm.localRotation = Quaternion.Euler(new Vector3(0f, 90f, MyStick.Degree + 90f));
            }
            else
            {
                RightArm.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 270f - MyStick.Degree));
                LeftArm.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 90f - MyStick.Degree));
            }
        }

        if (Input.GetKey(KeyCode.Z) || Bbutton.isClick)
        {
            GetComponent<Rigidbody>().GetComponent<ConstantForce>().force = Vector3.zero;
            if (GetComponent<Rigidbody>().velocity.y < 4f) GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * 20f);

            if (!Boost.loop)
            {
                Boost.Play();
                Boost.loop = true;
            }
            isMoving = true;
        }
        else
        {
            GetComponent<Rigidbody>().GetComponent<ConstantForce>().force = new Vector3(0f, -10f, 0f);
            Boost.loop = false;
        }

        if (Input.GetKey(KeyCode.X) || Abutton.isClick)
        {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Play();
                StartCoroutine("LightControl");
            }
            RightMuzzle.emissionRate = LeftMuzzle.emissionRate = 10;
            RightFire.emissionRate = LeftFire.emissionRate = 30;
        }
        else
        {
            GetComponent<AudioSource>().Stop();
            RightMuzzle.emissionRate = RightFire.emissionRate = LeftMuzzle.emissionRate = LeftFire.emissionRate = 0;
            LeftLight.intensity = RightLight.intensity = 0f;
            StopCoroutine("LightControl");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            LaunchMissile();
        }
        GetComponent<Rigidbody>().velocity = new Vector3(0f, GetComponent<Rigidbody>().velocity.y, 0f);
    }

    void LaunchMissile()
    {
        if (!LeanTween.isTweening(gameObject))
        {
            Vector3 pos = Vector3.zero;
            if (dir == Direction.RIGHT) pos = new Vector3(transform.position.x + 1.0f, transform.position.y + 1.0f, transform.position.z);
            if (dir == Direction.LEFT) pos = new Vector3(transform.position.x - 1.0f, transform.position.y + 1.0f, transform.position.z);

            for (int i = 0; i < 5; i++)
            {
                Vector3 origin = pos + Vector3.up * Random.Range(-1f, 1f) + Vector3.left * Random.Range(-1f, 1f);
                GameObject temp = Instantiate(Bomb, origin, Quaternion.AngleAxis(dir == Direction.RIGHT ? 0f : 180f, Vector3.up)) as GameObject;
                Vector3 tarPos = MissilePoint.position + MissilePoint.forward * 20f + MissilePoint.up * Random.Range(-1f, 1f);
                temp.SendMessage("LaunchMissile", tarPos);
            }
        }
    }

    void TurnLeft()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        dir = Direction.LEFT;
    }

    void TurnRight()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        dir = Direction.RIGHT;
    }
}
