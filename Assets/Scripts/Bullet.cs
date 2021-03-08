using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damageToDeal = 1;
    public float speed = 10f;
    public float lifeTime = 5f;

    public bool isEnemyBullet;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        CheckCollision();
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        
    }

    void CheckCollision()
    {
        
        RaycastHit hit;
        if(Physics.SphereCast(transform.position, 0.1f,transform.forward,out hit, 0.3f))
        {
            
            if(LayerMask.LayerToName(hit.transform.gameObject.layer) == "Enemy" && !isEnemyBullet)
            {

                hit.transform.GetComponent<BaseEnemy>().TakeDamage(damageToDeal);
                Destroy(gameObject);
            }
            if(LayerMask.LayerToName(hit.transform.gameObject.layer) == "Player" && isEnemyBullet)
            {
                hit.transform.GetComponent<Player>().TakeDamage(damageToDeal);
                Destroy(gameObject);
            }

        }
    }
    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.forward * 0.3f, 0.1f);
    }
}
