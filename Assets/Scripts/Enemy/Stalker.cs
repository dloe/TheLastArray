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
        Color trans = new Color(EnemyImage.color.r, EnemyImage.color.g, EnemyImage.color.b, 0.2f);
        EnemyImage.color = trans;
        hidden = true;
        yield return new WaitForSeconds(stealthTime);
        if(isObjectiveEnemy)
        {
            EnemyImage.color = Color.yellow;
        }
        else
        {
            EnemyImage.color = Color.white;
        }
        hidden = false;
    }

}
