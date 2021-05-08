using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : BaseEnemy
{
    public int teleTime;
    bool hidden;
    //public int range = 10;
    bool _InattackingMovement = false;
    float _speed;
   

    public override void specialAttack(Vector3 temp)
    {
        if(Vector3.Distance(_target.transform.position, this.transform.position) < 1.5f)
        {
            GetComponent<BoxCollider>().enabled = true;
            EnemyImage.enabled = true;
            GetComponent<Rigidbody>().useGravity = true;
            readyToAttack = false;
        }

        if (myState != enemyState.attacking && readyToAttack == true)
        {
            this.transform.position += temp * Time.deltaTime;
        }
        else if (myState == enemyState.attacking && readyToAttack == true)
        {
            this.transform.position += temp * Time.deltaTime;
            attacking = true;
            //  Debug.Log("running coro teleporting");
            if (!hidden && !_InattackingMovement && Vector3.Distance(this.transform.position, _target.transform.position) > attackDistance / 3)
            {
                _InattackingMovement = true;
                StartCoroutine(Teleport());
            }
            else
                attacking = false;
        }
        else if (myState == enemyState.attacking && readyToAttack == false)
        {
            this.transform.position += temp * Time.deltaTime;
        }

    }

    IEnumerator Teleport()
    {
        //GetComponent<MeshRenderer>().enabled = false;
        EnemyImage.enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        //yield return new WaitForSeconds(Random.Range(1, 3));
        _speed = baseSpeed;
        speed = 0;
        yield return new WaitForSeconds(Random.Range(1, teleTime));

        //shoots raycast forward of enemy
        //moves to where ever the ray hits
        //turn off colliders and mesh
        
        RaycastHit hit;
        
        if(Physics.Raycast(this.transform.position, transform.forward, out hit, attackDistance))
        {
            if(Vector3.Distance(transform.position, hit.point) < 1)
            {
                readyToAttack = true;
                hidden = false;
                _InattackingMovement = false;
                Debug.Log("check");
                attacking = false;
                StopCoroutine(Teleport());
            }
            
            
            Debug.Log("teleporting");
            hidden = true;
            Vector3 poi = hit.point;
            //move to that location

            Vector3 teleportPos = Vector3.Lerp(transform.position, poi, Random.Range(0.3f, 0.7f));

            transform.position = teleportPos;
            //Debug.Log("moving to teleportPos");
            GetComponent<BoxCollider>().enabled = true;
            EnemyImage.enabled = true;
            GetComponent<Rigidbody>().useGravity = true;

            //moves to that location
            yield return new WaitForSeconds(1.0f);
        }
        speed = _speed;
        hidden = false;

        // StartCoroutine(CoolDown());

        readyToAttack = true;

        yield return new WaitForSeconds(3.0f);
        _InattackingMovement = false;
    }


}
