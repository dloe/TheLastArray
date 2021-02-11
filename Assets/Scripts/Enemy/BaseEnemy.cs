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
    LayerMask _playerMask;
    float _angle = 0;
    public float disToTarget = 0;

    public float combatRadi;
    bool attacking = false;

    private void Start()
    {
        _tickRate = 2;
        _spawnPoint = transform.position;
        _mask = LayerMask.GetMask("Enviroment");
        _playerMask = LayerMask.GetMask("Player");
        StartCoroutine(Tick());

    }

    private void Update()
    {
        if (_target)
            disToTarget = Vector3.Distance(transform.position, _target.gameObject.transform.position);
        if (agro)
            Move(_target);
        else if (!agro)
            Wander(_spawnPoint, wanderSpeed, wonderRadius);
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

    /// <summary>
    /// this function will handle the movment of the enmey when agro is true
    /// </summary>
    private void Move(GameObject poi)
    {
        if (_target)
        {
            if (disToTarget >= combatRadi)//&& attacking = false)
                transform.position = Vector3.MoveTowards(transform.position, poi.transform.position, baseSpeed * Time.deltaTime);
            else if (disToTarget <= combatRadi)//&& attacking = false)
            {
                Wander(poi.transform.position, combatSpeed, combatRadi);
            }
        }


    }

    /// <summary>
    /// this will handle the base enemy movment on spawn
    /// </summary>
    void Wander(Vector3 poi, float speed, float radius)
    {
        //speed += Random.Range(-1000000, 1000000000);
        _angle += Time.deltaTime * speed;
        Vector3 offSet = new Vector3(Mathf.Cos(_angle), 0, Mathf.Sin(_angle)) * radius; //Mathf.Cos(_angle * Mathf.PI);
        transform.position = poi + offSet;
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
        Collider[] targets = Physics.OverlapSphere(transform.position, detectionRadius, _playerMask);
        if (targets.Length > 0)
        {
            _target = targets[0].gameObject;
            print("added target");
            OnAgro();
        }
        if (_target)
        {

            if (disToTarget >= agroLoseDis)
            {
                _target = null;
                agro = false;
            }
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_spawnPoint, wonderRadius);

        if (_target)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _target.transform.position);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_target.transform.position, combatRadi);
        }





    }
}
