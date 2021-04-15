using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stalker : BaseEnemy
{
    public int stealthTime;
    bool hidden;

    public override void specialAttack(Vector3 temp)
    {
        if (myState != enemyState.attacking && readyToAttack == true)
        {
            this.transform.position += temp * Time.deltaTime;
        }

        else if (myState == enemyState.attacking && readyToAttack == true)
        {
            this.transform.position += temp * Time.deltaTime;
            attacking = true;
            if(!hidden)
            StartCoroutine(Stealth());
        }
        else if (myState == enemyState.attacking && readyToAttack == false)
        {
            this.transform.position -= temp * Time.deltaTime;
        }
        
    }

    IEnumerator Stealth()
    {
        this.GetComponent<MeshRenderer>().material.color = Color.clear;
        hidden = true;
        yield return new WaitForSeconds(stealthTime);
        this.GetComponent<MeshRenderer>().material = norm;
        hidden = false;
    }

}
