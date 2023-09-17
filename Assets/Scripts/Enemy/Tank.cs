using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : BaseEnemy
{
    /// <summary>
    /// Tank Enemy Behaviors
    /// Alex
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - inherited from BaseEnemy parent
    /// </summary>
    
    public float knockback = 0;

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
            _target.GetComponent<Rigidbody>().AddForce(this.transform.forward * knockback * 10);
            _target.GetComponent<Rigidbody>().AddForce(Vector3.up * knockback * 5);
            _target.GetComponent<Player>().TakeDamage(baseAttack, baseMeleeKnockback, this.transform);

        }
        else if (myState == enemyState.attacking && readyToAttack == false)
        {
            this.transform.position -= temp * Time.deltaTime;
        }
        
        base.specialAttack(temp);
    }
}
