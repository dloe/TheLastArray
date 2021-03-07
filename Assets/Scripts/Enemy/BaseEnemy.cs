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

    [Header("Base_Stats")]
    // the base health of the enemy
    public int baseHealth = 5;

    //enemys base attack value
    public int baseAttack = 5;

    // the base move speed
    public float baseSpeed;

    // the radius around the enemy that it will ditect its target
    public float detectionRadius;

    // the internal timer used for the enemy makes it so not everything is not in update
    public float _tickRate;

    [Header("Movement_Stats")]

    public float avodinceRange = 2f;

    int rays = 25;

    float angle = 90;

    

    [Header("Wander_Stats")]
    // this is the speed at which the enemy will wonder around
    public float wanderSpeed;

    //This is the distance around its spawn the enemy will explore
    public float wonderRadius;

    //the starting point of the enemy
    Vector3 _spawnPoint;

    Vector3 pointToFollow = new Vector3();

    [Header("Combat_Stats")]
    // is the enemy agro
    public bool agro;

    //rate at which the attack will come out
    public float attackSpeed;

    // what is the enemy targting
    public GameObject _target;

    //how far the enemy needs to get away from its target to lose agro
    public float agroLoseDis;

    //how close the enemy needs to be to attack the player
    public float combatRadi;

    //is this enemy attacking
    bool attacking = false;


    private void Start()
    {
        _tickRate = 1;

        _spawnPoint = transform.position;


        StartCoroutine(Tick());

        //change soon
        _target = Player.Instance.gameObject;

    }

    private void Update()
    {
        //as of 3/7/21

        Stearing();


    }

    /// <summary>
    /// this will handles functions that do not need to be run in update 
    /// </summary>
    IEnumerator Tick()
    {
        //CheckSurondings();
        //MakePath();
        SetTarget();
        yield return new WaitForSeconds(_tickRate);
        StartCoroutine(Tick());
    }

   
    void Stearing()
    {
        Vector3 delta = Vector3.zero;

        for (int i = 0; i < rays; i++)
        {
            Quaternion rot = this.transform.rotation;
            var rotMod = Quaternion.AngleAxis((i / ((float)rays - 1)) * angle * 2 - angle, this.transform.up);
            var dir = rot * rotMod * Vector3.forward;


            Ray ray = new Ray(this.transform.position, dir);
            RaycastHit hitInfo;



            if (Physics.Raycast(ray, out hitInfo, avodinceRange))
            {
                delta -= (1f / rays) * baseSpeed * dir;
            }
            else
            {
                delta += (1f / rays) * baseSpeed * dir;
            }

            //print(hitInfo);
        }

        this.transform.position += delta * Time.deltaTime;
        this.transform.LookAt(_target.transform);
    }

    void BaseAttack()
    {

    }

    /// <summary>
    /// this function will be used to set the target
    /// </summary>
    void SetTarget()
    {

        /*
        if (disToTarget <= detectionRadius)
        {
            OnAgro();
        }

        else if (disToTarget >= agroLoseDis)
        {
            agro = false;
        }
        */
    }

    /// <summary>
    /// This will be called when the enemy is goes agro
    /// </summary>
    void OnAgro()
    {
        agro = true;
    }






    private void OnDrawGizmos()
    {
        for (int i = 0; i < rays; i++)
        {
            Quaternion rot = this.transform.rotation;
            var rotMod = Quaternion.AngleAxis((i / ((float)rays - 1)) * angle * 2 - angle, this.transform.up);
            var dir = rot * rotMod * Vector3.forward;
            Gizmos.DrawRay(this.transform.position, dir);
        }

        /*
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
        */

        //Gizmos.color = Color.magenta;
        //Gizmos.DrawSphere(pointToFollow, .5f);



    }



    //Old functions ignore
    void MakePath()// old
    {
        /*
        print("avoidence " + this.gameObject + "vector " + avoidence);
        avoidence = Vector3.zero;
        foreach (GameObject g in surrounding)
        {
            avoidence += g.transform.position;
        }
        */
    }

    void CheckSurondings()
    {
        /*
        surrounding.Clear();

        float radi = 2f;
        Collider[] hit = Physics.OverlapSphere(transform.position, radi, _mask);

        foreach (Collider thing in hit)
        {
            if (!surrounding.Contains(thing.gameObject))
            {
                surrounding.Add(thing.gameObject);
            }

        }

        */

    }

    void OldWander(Vector3 poi, float speed, float radius, float startingAngle, float timer)
    {
        /*
        timer = speed * timer;
        Vector3 offSet = new Vector3(Mathf.Cos(timer), 0, Mathf.Sin(timer)) * radius; //Mathf.Cos(_angle * Mathf.PI);
        pointToFollow = poi + offSet;
        transform.Translate(Vector3.right * Time.deltaTime * Mathf.Sin(Time.time * Mathf.PI), Space.World);
        transform.position = Vector3.MoveTowards(transform.position, pointToFollow, speed * 4 * Time.deltaTime);
        */
    }

    void oldUpdate()
    {
        /* old system will move to bottom of script
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
        */
    }

    private void Move(Vector3 poi, float disToPoi, float radi, float rotationSpeed, float angleBetweenObjects, float timer)
    {
        /*
        float timeTemp;
        if (agro && !phase)
        {
            phase = true;

            Vector3 offset = transform.position - poi - avoidence;

            if (!float.IsNaN(Mathf.Acos(offset.x)))
                timerAgro = -Mathf.Acos(offset.x);
            // Debug.Log(Mathf.Acos(offset.x));
            //  if (!float.IsNaN(Mathf.Asin(offset.z)))
            //      Debug.Log(Mathf.Asin(offset.z));
            timeTemp = timerAgro;

        }
        else
            timeTemp = timer;

        if (agro)
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
        */
    }

}
