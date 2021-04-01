using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warden : BaseEnemy
{
    [Header ("Wardens special mchanics")]
    public GameObject sprout;
    public GameObject spawnpoint;
    public int spawnrate;

    public override void specialAttack(Vector3 temp)
    {
        if (myState != enemyState.attacking && readyToAttack == true)
        {
            this.transform.position += temp * Time.deltaTime;
        }

        else if (myState == enemyState.attacking && readyToAttack == true)
        {
            this.transform.position += temp * Time.deltaTime;
            StartCoroutine( Swarm());
            
        }
        else if (myState == enemyState.attacking && readyToAttack == false)
        {
            this.transform.position -= temp * Time.deltaTime;
        }
        
    }

    IEnumerator Swarm()
    {
        Instantiate(sprout, spawnpoint.transform.position, this.transform.rotation);
        readyToAttack = false;
        yield return new WaitForSeconds(spawnrate);
        readyToAttack = true;
    }

}
