using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisenCloud : MonoBehaviour
{

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
