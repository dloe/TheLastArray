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
        Instantiate(sprout, spawnpoint.transform.position, Quaternion.Euler(Player.Instance.transform.parent.rotation.eulerAngles.x, -Player.Instance.transform.parent.rotation.eulerAngles.y, Player.Instance.transform.parent.rotation.eulerAngles.z));
        readyToAttack = false;
        yield return new WaitForSeconds(spawnrate);
        readyToAttack = true;
    }

}
