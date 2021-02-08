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

    private void Start()
    {
        _tickRate = 5;
        _spawnPoint = transform.position;
        _mask = LayerMask.GetMask("Enviroment");
        _playerMask = LayerMask.GetMask("Player");
        StartCoroutine(Tick());

    }

    private void Update()
    {
        Move();
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
    /// this function will handle the movment of the enmey
    /// </summary>
    private void Move()
    {
        if(_target != null)
        {
            float mag = Vector3.Distance(transform.position, _target.gameObject.transform.position);
            print(mag);
            if(mag >= agroLoseDis)
            {
                _target = null;
            }
        }
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
        if(targets.Length > 0)
        {
            _target = targets[0].gameObject;
        }



    }

    /// <summary>
    /// This will be called when the enemy is goes agro
    /// </summary>
    void OnAgro()
    {

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





    }
}
