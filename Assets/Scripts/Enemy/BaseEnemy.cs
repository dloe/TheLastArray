﻿/* Author: Alex Olah
 * Date 2/6/2021
 * This is the base funtinalty of all enemy types in the game
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public enum EnemyType
{
    basic
}

public enum AttackType
{
    none,
    melee,
    ranged
}

public enum enemyState
{
    wandering,
    following,
    attacking
}

public class BaseEnemy : MonoBehaviour
{
    [HideInInspector]
    public int imageDirMod = 1;
    public Image EnemyImage;
    public Material norm;
    public Material damaged;
    public Material key;

    [Header("Base_Stats")]

    public bool isObjectiveEnemy;

    //what enemy am I
    public EnemyType myType;

    public enemyState myState = enemyState.wandering;

    public bool notWarden = false;

    // the base health of the enemy
    public int baseHealth = 5;

    //enemys base attack value
    public int baseAttack = 5;

    public float baseMeleeKnockback = 10f;

    // the base move speed
    public float baseSpeed;

    // the radius around the enemy that it will direct its target
    public float detectionRadius;

    // the internal timer used for the enemy makes it so not everything is not in update
    public float tickRate;

    //current object that has the enemy's attention
    Vector3 _poi;

    [Header("Movement_Stats")]

    public float avodinceRange = 2f;

    int _rays = 25;

    float _angle = 90;



    [Header("Wander_Stats")]
    // this is the speed at which the enemy will wonder around
    public float wanderSpeed;

    //This is the distance around its spawn the enemy will explore
    public float wanderRadius;

    //the starting point of the enemy
    Vector3 _spawnPoint;

    Vector3 _wanderPoint = new Vector3();

    [Header("Combat_Stats")]
    //how do I attack
    public AttackType attackType;

    // is the enemy agro
    public bool agro = false;

    //rate at which the attack will come out
    public float attackSpeed;

    float _attackCD = 0;


    public float combatSpeed;

    //how close the enemy needs to be to attack the player
    public float attackDistance;

    // what is the enemy targeting
    public GameObject _target;

    public bool attacking;

    //is this enemy attacking
    public bool readyToAttack = false;

    //how far the enemy needs to get away from its target to lose agro
    public float agroLoseDis;

    [Header("if melee enemy")]
    //how far the attack will go
    public float attackRange;

    [Header("if ranged enemy")]
    public GameObject projectile;

    [Header("Audio")]
    public AudioClip[] agroSound;
    public AudioClip[] takeDamageSound;
    public AudioClip rifleSound;
    [Space(25)]
    AudioSource _audioSource;
    bool _audioPrevent = true;
    protected float speed;

    bool _onAgroStart = false;
    [Header("Minimap Objective Marker")]
    public GameObject objectiveMinimapMarker;

    float _x;
    float _z;

    bool _start = false;

    public virtual void Start()
    {
        StartCoroutine(AgroStartCoolDown());
        _audioSource = GetComponent<AudioSource>();
        _spawnPoint = transform.position;
        _wanderPoint = _spawnPoint;
        _poi = _wanderPoint;
        _target = Player.Instance.gameObject;
        StartCoroutine(Tick());
        if(isObjectiveEnemy)
        {
            EnemyImage.color = Color.yellow;
            objectiveMinimapMarker.SetActive(true);
        }
    }


    private void Update()
    {
        //as of 3/7/21
        Atention();
        Stearing();
        WanderingPoint();

        if (baseHealth <= 0)
        { OnDeath(); }

        if(agro == true && _audioPrevent)
        {
           // PlayAgroSound();
            //audio cooldown
            StartCoroutine(AudioCoolDown());
        }
    }

    IEnumerator AudioCoolDown()
    {
        _audioPrevent = false;
        yield return new WaitForSeconds(10f);
        _audioPrevent = true;
    }

    //spawning system causes enemies to agro immediately on startup (wait a few seconds before they can agro)
    IEnumerator AgroStartCoolDown()
    {
        yield return new WaitForSeconds(2.0f);
        _onAgroStart = true;
    }
    /// <summary>
    /// this will handles functions that do not need to be run in update 
    /// </summary>
    IEnumerator Tick()
    {
        BaseAttack();
        SetTarget();
        StateChanger();
        yield return new WaitForSeconds(tickRate);
        StartCoroutine(Tick());
    }


    void Stearing()
    {
        speed = 0;
        switch (myState)
        {
            case enemyState.wandering:
                speed = wanderSpeed;
                break;
            case enemyState.following:
                speed = baseSpeed;
                break;
            case enemyState.attacking:
                speed = combatSpeed;
                break;

        }
        Vector3 delta = Vector3.zero;



        for (int i = 0; i < _rays; i++)
        {
            Quaternion rot = this.transform.rotation;
            var rotMod = Quaternion.AngleAxis((i / ((float)_rays - 1)) * _angle * 2 - _angle, this.transform.up);
            var dir = rot * rotMod * Vector3.forward;

            Ray ray = new Ray(this.transform.position, dir);
            RaycastHit hitInfo;



            if (Physics.Raycast(ray, out hitInfo, avodinceRange) && !attacking )
            { delta -= (1f / _rays) * speed * dir; }
            else
            { delta += (1f / _rays) * speed * dir; }


        }


        switch (attackType)
        {
            case AttackType.none:
                specialAttack(delta);
                break;
            case AttackType.melee:
                if (myState != enemyState.attacking && readyToAttack == true)
                {
                    this.transform.position += delta * Time.deltaTime;
                }
                
                else if (myState == enemyState.attacking && readyToAttack == true)
                {
                    this.transform.position += delta * Time.deltaTime;
                    attacking = true;
                }
                else if (myState == enemyState.attacking && readyToAttack == false)
                {
                    this.transform.position -= delta * Time.deltaTime;
                }
                break;
            case AttackType.ranged:
                if(myState != enemyState.attacking && readyToAttack == true)
                {
                    this.transform.position += delta * Time.deltaTime;
                }
                
                else if (myState == enemyState.attacking && readyToAttack == true)
                {
                    
                    attacking = true;
                }
                else if (myState == enemyState.attacking && readyToAttack == false)
                {
                    this.transform.position -= delta * Time.deltaTime;
                }
                break;
                

        }


        this.transform.LookAt(new Vector3(_poi.x,this.transform.position.y,_poi.z));
        if(EnemyImage != null)
        {
            Vector3 localLook = Quaternion.AngleAxis(EnemyImage.transform.parent.rotation.eulerAngles.y, Vector3.up) * (imageDirMod*(_poi - transform.position).normalized);
            if (localLook.x < 0.01)
            {
                EnemyImage.transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (localLook.x >= 0)
            {
                EnemyImage.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        
    }
    public virtual void specialAttack(Vector3 temp)
    {

    }

    void StateChanger()
    {
        if (Vector3.Distance(this.transform.position, _spawnPoint) < wanderRadius && agro == false)
        {
            myState = enemyState.wandering;
        }
        if (Vector3.Distance(this.transform.position, _target.transform.position) <= attackDistance)
        {
            myState = enemyState.attacking;
        }
        if (Vector3.Distance(this.transform.position, _target.transform.position) >= attackDistance  && agro == true)
        {
            myState = enemyState.following;
            attacking = false;
        }
    }

    void BaseAttack()
    {
        RaycastHit attackRay;
        if (attackType == AttackType.melee || (attackType == AttackType.none && notWarden))
        {
            if (Physics.BoxCast(this.transform.position, Vector3.zero, transform.forward, out attackRay, transform.rotation, attackRange))
            {
                if (LayerMask.LayerToName(attackRay.transform.gameObject.layer) == "Player" && _attackCD <= 0 && myState == enemyState.attacking)
                {
                    attacking = false;
                    readyToAttack = false;
                    _attackCD = attackSpeed;
                    StartCoroutine(CoolDown());
                    attackRay.transform.GetComponent<Player>().TakeDamage(baseAttack, baseMeleeKnockback, this.transform);
                    
                    //Debug.Log(this.gameObject.name + ": HitPlayer for " + baseAttack);
                }
            }

        }
        else if (attackType == AttackType.ranged)
        {
            if(myState == enemyState.attacking && _attackCD <= 0 && readyToAttack == true)
            {
                _audioSource.clip = rifleSound;
                _audioSource.Play();
                attacking = false;
                readyToAttack = false;
                StartCoroutine(CoolDown());
                //Debug.Log("Bang Bang");
                GameObject bullet = Instantiate(projectile, transform.position, transform.rotation);
                if(notWarden == false)
                bullet.GetComponent<Bullet>().damageToDeal = baseAttack;

            }
        }
    }
    IEnumerator CoolDown()
    {
        //Debug.Log("cooling down");
        yield return new WaitForSeconds(attackSpeed);
        //Debug.Log("ready to attack");
        _attackCD = 0;
        readyToAttack = true;

    }

    /// <summary>
    /// this function will be used to set agro
    /// </summary>
    void SetTarget()
    {
        if (Vector3.Distance(transform.position, _target.gameObject.transform.position) <= detectionRadius && _onAgroStart)
        {
            //Debug.Log(this.transform.position);
            agro = true;
            
            myState = enemyState.following;
        }

        else if (Vector3.Distance(transform.position, _target.gameObject.transform.position) >= agroLoseDis)
        {
            agro = false;

        }
    }

    void Atention()
    {
        if (!agro)
        { _poi = _wanderPoint; }
        else if (agro)
        { _poi = _target.transform.position; }
    }

    /// <summary>
    /// This will be called when the enemy is goes agro
    /// </summary>
    void onAgro()
    {

    }


    void WanderingPoint()
    {


        if (_start == false)
        {
            _x = Random.Range(-1f, 1f);
            _z = Random.Range(-1f, 1f);

            _start = true;
        }

        if (Vector3.Distance(_wanderPoint, _spawnPoint) >= wanderRadius)
        {
            ChangeDirW();
        }
        _wanderPoint += new Vector3(_x, 0, _z).normalized * (wanderSpeed * 1.5f) * Time.deltaTime;
    }
    void ChangeDirW()
    {
        _x = -_x;
        _z = -_z;
    }

    public void TakeDamage(int damage, Transform instigator, float damageKnockBack)
    {
        TakeDamageAudio();
        baseHealth -= damage;

        if(instigator != null)
        {
            float kbMultiplier = damage;
            if(damageKnockBack != 0)
            {
                kbMultiplier = damageKnockBack;
            } else if(damageKnockBack < 0) { 
                kbMultiplier = 0.0f;
            }

            //push back enemy
            //for now might try to have a basic scale with damage
            gameObject.GetComponent<Rigidbody>().AddForce(instigator.forward * kbMultiplier);
        }
        

        StartCoroutine(Damaged());
    }

    public void TakeFireDamge(int dmgPerSecond)
    {
        StartCoroutine(TakeFireDamageCR(dmgPerSecond));
    }

    public IEnumerator TakeFireDamageCR(int dmgPerSecond)
    {
        for(int i = 0; i < 3; i++)
        {
            Debug.Log("Fire damage " + i);
            TakeDamage(dmgPerSecond, null, -1.0f);
            yield return new WaitForSeconds(1f);
        }
    }
    IEnumerator Damaged()
    {
        if (!isObjectiveEnemy)
        {
            if(EnemyImage != null)
            {
                EnemyImage.color = Color.red;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.white;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.red;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.white;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.red;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.white;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.red;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.white;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.red;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.white;
            }
            else
            {
                this.GetComponent<MeshRenderer>().material = damaged;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = norm;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = damaged;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = norm;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = damaged;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = norm;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = damaged;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = norm;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = damaged;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = norm;
            }
            
        }
        if (isObjectiveEnemy)
        {
            if (EnemyImage != null)
            {
                EnemyImage.color = Color.red;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.yellow;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.red;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.yellow;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.red;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.yellow;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.red;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.yellow;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.red;
                yield return new WaitForSeconds(.1f);
                EnemyImage.color = Color.yellow;
            }
            else
            {
                this.GetComponent<MeshRenderer>().material = damaged;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = key;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = damaged;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = key;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = damaged;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = key;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = damaged;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = key;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = damaged;
                yield return new WaitForSeconds(.1f);
                this.GetComponent<MeshRenderer>().material = key;
                yield return new WaitForSeconds(.1f);
            }
            

        }
    }

    void TakeDamageAudio()
    {

        int aIndex = Random.Range(0, takeDamageSound.Length);
        _audioSource.clip = takeDamageSound[aIndex];

        _audioSource.Play();
    }

    public virtual void OnDeath()
    {
        if (isObjectiveEnemy)
        {
            Objectives.Instance.SendCompletedMessage(Condition.KillEnemy);
        }
        Destroy(this.gameObject);
    }


    private void OnDrawGizmos()
    {
        for (int i = 0; i < _rays; i++)
        {
            Quaternion rot = this.transform.rotation;
            var rotMod = Quaternion.AngleAxis((i / ((float)_rays - 1)) * _angle * 2 - _angle, this.transform.up);
            var dir = rot * rotMod * Vector3.forward;
            Gizmos.DrawRay(this.transform.position, dir);
        }



        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_spawnPoint, wanderRadius);

        //Gizmos.color = Color.blue;
        // Gizmos.DrawLine(transform.position, _spawnPoint);


        if (agro)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _target.transform.position);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_target.transform.position, attackDistance);
        }

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(_poi, .1f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_wanderPoint, .2f);



    }

    /// <summary>
    /// Plays sound when enemy becomes agro
    /// </summary>
    void PlayAgroSound()
    {
        //Debug.Log("Play audio");
        int soundI = Random.Range(0, agroSound.Length);
        
        _audioSource.clip = agroSound[soundI];
        _audioSource.Play();
        //_audioSource.PlayClipAtPoint(agroSound[soundI], transform.position, 1);
    }

    //Old functions ignore
    /*
    void MakePath()// old
    {
        /*
        print("avoidance " + this.gameObject + "vector " + avoidence);
        avoidence = Vector3.zero;
        foreach (GameObject g in surrounding)
        {
            avoidence += g.transform.position;
        }
        
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

        

    }

    void OldWander(Vector3 poi, float speed, float radius, float startingAngle, float timer)
    {
        /*
        timer = speed * timer;
        Vector3 offSet = new Vector3(Mathf.Cos(timer), 0, Mathf.Sin(timer)) * radius; //Mathf.Cos(_angle * Mathf.PI);
        pointToFollow = poi + offSet;
        transform.Translate(Vector3.right * Time.deltaTime * Mathf.Sin(Time.time * Mathf.PI), Space.World);
        transform.position = Vector3.MoveTowards(transform.position, pointToFollow, speed * 4 * Time.deltaTime);
        
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
        
    }
    */


}
