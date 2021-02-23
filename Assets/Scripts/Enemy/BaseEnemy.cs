/* Author: Alex Olah
 * Date 2/6/2021
 * This is the base funtinalty of all enemy types in the game
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    //EnemyType basic

    //AttackType null

    // the base health of the enemy
    public int baseHealth = 5;

    //enemys base attack value
    public int baseAttack = 5;

    // the base move speed
    public float baseSpeed;

    // this is the speed at which the enemy will wonder around
    public float wanderSpeed;

    public float combatSpeed;

    // the radius around the enemy that it will ditect its target
    public float detectionRadius;

    // the internal timer used for the enemy makes it so not everything is not in update
    int _tickRate;

    // what is the enemy targting
    public GameObject _target;

    // is the enemy agro
    public bool agro;

    //how far the enemy needs to get away from its target to lose agro
    public float agroLoseDis;

    //the starting point of the enemy
    Vector3 _spawnPoint;

    //This is the distance around its spawn the enemy will explore
    public float wonderRadius;

    //objects that are being avoided 
    public List<GameObject> surrounding = new List<GameObject>();

    //this is the layer that the enemy will avoide 
    LayerMask _mask;

    public float _timer = 0;
    public float disToTarget, disToSpawn;

    public float angleToTarget, angleToSpawn;

    public float combatRadi;
    bool attacking = false;

    Vector3 pointToFollow = new Vector3();

    private void Start()
    {
        _tickRate = 2;

        _spawnPoint = transform.position;

        _mask = LayerMask.GetMask("Enviroment");

        disToTarget = Vector3.Distance(transform.position, _target.gameObject.transform.position);

        StartCoroutine(Tick());

        _target = Player.Instance.gameObject;

    }

    private void Update()
    {
        disToTarget = Vector3.Distance(transform.position, _target.gameObject.transform.position);
        disToSpawn = Vector3.Distance(transform.position, _spawnPoint);


        _timer += Time.deltaTime;

        Vector3 temp = transform.position - _target.transform.position;
        angleToTarget = Mathf.Atan2(temp.x, temp.z) * Mathf.Rad2Deg;

        Vector3 temp2 = transform.position - _spawnPoint;
        angleToSpawn = Mathf.Atan2(temp2.x, temp2.z) * Mathf.Rad2Deg;


        if (agro)
        {
            Move(_target.transform.position, disToTarget, combatRadi, combatSpeed, angleToTarget, _timer);
            transform.LookAt(_target.transform);

        }
        else if (!agro)
        {
            timerAgro = 0;
            phase = false;
            Move(_spawnPoint, disToSpawn, wonderRadius, wanderSpeed, angleToSpawn, _timer);
        }
    }

    /// <summary>
    /// this will handles functions that do not need to be run in update 
    /// </summary>
    IEnumerator Tick()
    {
        CheckSurondings();
        SetTarget();
        yield return new WaitForSeconds(_tickRate);
        StartCoroutine(Tick());
    }
    public bool phase = false;
    public float timerAgro = 0;
    /// <summary>
    /// this function will handle the movment of the enmey when agro is true  WRONGS
    /// </summary>
    private void Move(Vector3 poi, float disToPoi, float radi, float rotationSpeed, float angleBetweenObjects, float timer)
    {
        float timeTemp;
        if (agro && !phase)
        {
            phase = true;

            Vector3 offset = transform.position - poi;


            
            if (!float.IsNaN(Mathf.Acos(offset.x)))
                timerAgro = -Mathf.Acos(offset.x);
            // Debug.Log(Mathf.Acos(offset.x));
          //  if (!float.IsNaN(Mathf.Asin(offset.z)))
          //      Debug.Log(Mathf.Asin(offset.z));
            timeTemp = timerAgro;

        }
        else
            timeTemp = timer;

        if(agro)
            timerAgro += Time.deltaTime;

        if (disToPoi >= radi)
        {
            transform.position = Vector3.MoveTowards(transform.position, poi, baseSpeed * Time.deltaTime);
            //phase = false;
        }
        else if (disToPoi <= radi)//&& attacking = false)
        {
            


            Wander(poi, rotationSpeed, radi, angleBetweenObjects, timeTemp);
        }
    }

    /// <summary>
    /// this will handle the base enemy movment on spawn BADDDDDDDDD
    /// </summary>
    void Wander(Vector3 poi, float speed, float radius, float startingAngle, float timer)
    {
        timer = speed * timer;
        Vector3 offSet = new Vector3(Mathf.Cos(timer), 0, Mathf.Sin(timer)) * radius; //Mathf.Cos(_angle * Mathf.PI);
        pointToFollow = poi + offSet;
        transform.Translate(Vector3.right * Time.deltaTime * Mathf.Sin(Time.time * Mathf.PI), Space.World);
        transform.position = Vector3.MoveTowards(transform.position, pointToFollow, speed * 4 * Time.deltaTime);
    }

    /// <summary>
    /// the normal attack function of the enemy
    /// </summary>
    void BaseAttack()
    {

    }

    /// <summary>
    /// this function will be used to set the target
    /// </summary>
    void SetTarget()
    {
        if (disToTarget <= detectionRadius)
        {
            OnAgro();
        }

        else if (disToTarget >= agroLoseDis)
        {
            agro = false;
        }
    }

    /// <summary>
    /// This will be called when the enemy is goes agro
    /// </summary>
    void OnAgro()
    {
        agro = true;
    }

    void MakePath()
    {

    }

    /// <summary>
    /// Using rays from the enemy this will make sure it is not runing into anything
    /// </summary>
    void CheckSurondings()
    {

        float radi = 2f;
        Collider[] hit = Physics.OverlapSphere(transform.position, radi, _mask);

        foreach (Collider thing in hit)
        {
            if (!surrounding.Contains(thing.gameObject))
                surrounding.Add(thing.gameObject);
        }



    }




    private void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2f);
        if (!agro)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_spawnPoint, wonderRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _spawnPoint);
        }

        if (agro)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _target.transform.position);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_target.transform.position, combatRadi);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(pointToFollow, .5f);



    }
}
