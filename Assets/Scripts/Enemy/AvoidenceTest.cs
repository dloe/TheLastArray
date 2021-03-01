using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidenceTest : MonoBehaviour
{
    public float speed = 10;

    int rays = 11;

    float angle = 90;


    public float range = 2;


    private void Update()
    {

        Vector3 delta = Vector3.zero;

        for (int i = 0; i < rays; i++)
        {
            Quaternion rot = this.transform.rotation;
            var rotMod = Quaternion.AngleAxis((i / ((float)rays - 1)) * angle * 2 - angle, this.transform.up);
            var dir = rot * rotMod * Vector3.forward;


            Ray ray = new Ray(this.transform.position, dir);
            RaycastHit hitInfo;

            

            if (Physics.Raycast(ray, out hitInfo, range))
            {
                delta -= (1f / rays) * speed * dir;
            }
            else
            {
                delta += (1f / rays) * speed * dir;
            }

            print(hitInfo);
        }

        this.transform.position += delta * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < rays; i++)
        {
            Quaternion rot = this.transform.rotation;
            var rotMod = Quaternion.AngleAxis((i / ((float)rays - 1)) * angle * 2 - angle, this.transform.up);
            var dir = rot * rotMod * Vector3.forward;
            Gizmos.DrawRay(this.transform.position, dir);
        }
    }
}
