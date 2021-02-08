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
    GameObject _target;

    // is the enemy agro
    public bool agro;

    //how far the enemy needs to get away from its target to lose agro
    public float agroLoseDis;

    //the starting point of the enemy
    Vector3 _spawnPoint;

    //This is the distance around its spawn the enemy will explore
    public float wonderRadius;



    private void Start()
    {
        _spawnPoint = transform.position;

    }

    private void Update()
    {
        
    }

    /// <summary>
    /// this will handles functions that do not need to be run in update 
    /// </summary>
    private void Tick()
    {

    }

    /// <summary>
    /// this function will handle the movment of the enmey
    /// </summary>
    private void Move()
    {
        
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

    }

    /// <summary>
    /// This will be called when the enemy is goes agro
    /// </summary>
    void OnAgro()
    {

    }

    /// <summary>
    /// Using rays from the enemy this will make sure it is not runing into anything
    /// </summary>
    void CheckSurondings()
    {
        RaycastHit ray;

        

    }




    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_spawnPoint, wonderRadius);

        RaycastHit ray;
        float surrounding = 2f;

        bool isHit = Physics.SphereCast(transform.position,)


        
    }
}
