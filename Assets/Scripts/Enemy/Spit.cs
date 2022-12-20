using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spit : MonoBehaviour
{
    /// <summary>
    /// Spit Enemy Behaviors
    /// Alex
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - inherited from BaseEnemy parent
    /// </summary>
    
    public int speed;
    public GameObject gas;
    int _lifeTime = 4;

    private void Start()
    {
        Destroy(gameObject, _lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Enemy")
        {
            Instantiate(gas, this.transform.position, this.transform.rotation);
            Destroy(this.gameObject);
        }
    }


}
