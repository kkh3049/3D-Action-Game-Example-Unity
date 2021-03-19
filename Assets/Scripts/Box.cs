using UnityEngine;
using System.Collections;

public class Box : MonoBehaviour
{
    void OnParticleCollision(GameObject tar)
    {
        if (tar.name == "Muzzle")
        {
            GetComponent<Rigidbody>().AddForce(tar.transform.forward * 800f);
        }
    }
}
