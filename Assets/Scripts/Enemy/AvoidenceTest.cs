using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidenceTest : MonoBehaviour
{
    /// <summary>
    /// Avoidance Test
    /// Alex
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - 
    /// </summary>
    public float speed = 10;

    int _rays = 25;

    float _angle = 90;


    public float range = 2;

    public GameObject target;


    private void Update()
    {

        Vector3 delta = Vector3.zero;

        for (int i = 0; i < _rays; i++)
        {
            Quaternion rot = this.transform.rotation;
            var rotMod = Quaternion.AngleAxis((i / ((float)_rays - 1)) * _angle * 2 - _angle, this.transform.up);
            var dir = rot * rotMod * Vector3.forward;


            Ray ray = new Ray(this.transform.position, dir);
            RaycastHit hitInfo;

            

            if (Physics.Raycast(ray, out hitInfo, range))
            {
                delta -= (1f / _rays) * speed * dir;
            }
            else
            {
                delta += (1f / _rays) * speed * dir;
            }

            print(hitInfo);
        }

        this.transform.position += delta * Time.deltaTime;
        this.transform.LookAt(target.transform);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < _rays; i++)
        {
            Quaternion rot = this.transform.rotation;
            var rotMod = Quaternion.AngleAxis((i / ((float)_rays - 1)) * _angle * 2 - _angle, this.transform.up);
            var dir = rot * rotMod * Vector3.forward;
            Gizmos.DrawRay(this.transform.position, dir);
        }
    }
}
