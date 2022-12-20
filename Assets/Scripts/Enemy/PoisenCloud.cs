using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisenCloud : MonoBehaviour
{
    /// <summary>
    /// Poison Cloud Behaviors
    /// Alex
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - when touching player, they take damage
    /// - lingers for a short time (lifeTime)
    /// </summary>
    
    public int lifeTime = 10;
    public int damage;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            other.GetComponent<Player>().poisned(damage);
        }
    }



}
