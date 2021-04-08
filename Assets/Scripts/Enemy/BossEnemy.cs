using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class BossEnemy : MonoBehaviour
{
    public enum bossPhases
    {
        phase1,
        phase2
    };

    public bossPhases mbossPhase;
    public enum attackTypes
    {
       none,
       firstAttack,
       secondAttack,
       thirdAttack,
       forthAttack
    };
    
    public Material norm;
    public Material damaged;

    [Header("Base_Stats")]

    //what enemy am I
    //public EnemyType myType;

    public enemyState myState = enemyState.wandering;

    // the base health of the enemy
    public int baseHealth = 100;

    //enemys base attack value - changes based on attack
    int baseAttack = 5;

    // the base move speed
    float baseSpeed = 5;

    // the radius around the enemy that it will ditect its target
    float detectionRadius = 16;

    // the internal timer used for the enemy makes it so not everything is not in update
    float tickRate = 0.5f;

    //currect object that has the enemys attention
    Vector3 poi;

    [Header("Movement_Stats")]

    public float avodinceRange = 2f;

    int rays = 25;
    float angle = 90;

    [Header("Wander_Stats")]
    // this is the speed at which the enemy will wonder around
    public float wanderSpeed;

    //This is the distance around its spawn the enemy will explore
    public float wanderRadius;

    //the starting point of the enemy
    Vector3 _spawnPoint;

    Vector3 wanderPoint = new Vector3();

    [Header("Combat_Stats")]
    //how do I attack
    //public AttackType attackType;
    public attackTypes mAttackType;

    // is the enemy agro
    public bool agro;

    //rate at which the attack will come out
    public float attackSpeed;

    float attackCD = 0;

    public float combatSpeed;

    //how close the enemy needs to be to attack the player
    public float attackDistance;

    // what is the enemy targting
    public GameObject _target;

    [Header("is attacking?")]
    public bool attacking;

    //is this enemy attacking
    [Header("boss ready to strike")]
    public bool readyToAttack = true;

    public bool _currentlyInAttackMovement = false;
    public float playerDistanceFromBoss;
    //how far the enemy needs to get away from its target to lose agro
    public float agroLoseDis;

    [Header("if melee")]
    //how far the attack will go
    public float attackRange;

    [Header("if ranged enemy")]
    public GameObject projectile;

    [Header("Boss apendages")]
    public GameObject attack1_hand;
    public GameObject attack2_leftswipe;
    public GameObject attack2_rightswipe;
    public GameObject minion;

    public GameObject[] bulletRing;
    

    private void Start()
    {

        _spawnPoint = transform.position;
        wanderPoint = _spawnPoint;
        poi = wanderPoint;
        _target = Player.Instance.gameObject;
        StartCoroutine(Tick());

    }

    private void Update()
    {
        //as of 3/7/21
        Atention();
        Stearing();
        WanderingPoint();

        if (baseHealth <= 0)
        { OnDeath(); }

        if (baseHealth <= 30)
            mbossPhase = bossPhases.phase2;

        playerDistanceFromBoss = (transform.position - poi).magnitude;
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
        float speed = 0;
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

        for (int i = 0; i < rays; i++)
        {
            Quaternion rot = this.transform.rotation;
            var rotMod = Quaternion.AngleAxis((i / ((float)rays - 1)) * angle * 2 - angle, this.transform.up);
            var dir = rot * rotMod * Vector3.forward;

            Ray ray = new Ray(this.transform.position, dir);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, avodinceRange) && !attacking)
            { delta -= (1f / rays) * speed * dir; }
            else
            { delta += (1f / rays) * speed * dir; }
        }

        switch (mAttackType)
        {
            case attackTypes.none:
                //idea: if boss is to close to player, dont move towards it anymore
                this.transform.position += delta * Time.deltaTime;
                break;
            case attackTypes.firstAttack:
                if (!_currentlyInAttackMovement)
                {
                    if (myState != enemyState.attacking && readyToAttack == true)
                    {
                        this.transform.position += delta * Time.deltaTime;
                    }
                    else if (myState == enemyState.attacking && readyToAttack == true)
                    {
                        this.transform.position += delta * Time.deltaTime;
                        attacking = true;
                    }
                }
                break;
            case attackTypes.secondAttack:
             //   if (myState != enemyState.attacking && readyToAttack == true)
            //    {
             //       this.transform.position += delta * Time.deltaTime;
             //   }
                 if (myState == enemyState.attacking && readyToAttack == true)
                {
                   // this.transform.position += delta * Time.deltaTime;
                    attacking = true;
                }
                break;
            case attackTypes.thirdAttack:
                if(mbossPhase == bossPhases.phase2)
                {
                    if (myState != enemyState.attacking && readyToAttack == true)
                    {
                        this.transform.position += delta * Time.deltaTime;
                    }
                    else if (myState == enemyState.attacking && readyToAttack == true)
                    {
                        this.transform.position += delta * Time.deltaTime;
                        attacking = true;
                    }
                }
                else
                {
                    if (myState == enemyState.attacking && readyToAttack == true)
                    {
                       // this.transform.position += delta * Time.deltaTime;
                        attacking = true;
                    }
                    if (playerDistanceFromBoss < 6)
                    {
                        if(myState == enemyState.attacking)
                            this.transform.position -= delta * Time.deltaTime;
                    }
                }
                    
                //doesnt move if spawning shit
                break;
            default:
                break;
        }

        if(!_currentlyInAttackMovement)
            this.transform.LookAt(poi);
    }

    void StateChanger()
    {
        if (Vector3.Distance(this.transform.position, _target.transform.position) <= attackDistance)
        {
            myState = enemyState.attacking;
            //AttackStateChanger();
            //maybe BaseAttack();
        }
        if (Vector3.Distance(this.transform.position, _target.transform.position) >= attackDistance && agro == true)
        {
            myState = enemyState.following;
            attacking = false;
        }
    }

    //determines which attack we use when we attack
    void BaseAttack()
    {
        //randomly pick from 3 attacks
        //if player is in certain distance, have weight  be more for close range
        if (myState == enemyState.attacking && readyToAttack)
        {
            //first determine player distance from baddie
            //LocatePlayer();

            if (playerDistanceFromBoss <= 4.5)
            {
                //more weight to close range attacks
                if (mbossPhase == bossPhases.phase1)
                {
                    //40 - 40 - 20
                    if (Random.value > 0.6f)
                    {
                        if (Random.value > 0.5f)
                        {
                            mAttackType = attackTypes.firstAttack;
                            ThrustHandAttack();
                        }
                        else
                        {
                            mAttackType = attackTypes.secondAttack;
                            BulletRingAttack();
                        }
                    }
                    else
                    {
                        mAttackType = attackTypes.thirdAttack;
                        //spawn stuff
                        SpawnMinions();

                    }
                }
                else
                {
                    //besserk mode
                    //40 - 40 - 20
                    if (Random.value > 0.6f)
                    {
                        if (Random.value > 0.5f)
                        {
                            mAttackType = attackTypes.firstAttack;
                            ThrustHandAttack();
                        }
                        else
                        {
                            mAttackType = attackTypes.secondAttack;
                            BulletRingAttack();
                        }
                    }
                    else
                    {
                        mAttackType = attackTypes.thirdAttack;
                        SlamAttack();
                    }
                }
            }
            else
            {
                //player farther from player

                if (mbossPhase == bossPhases.phase1)
                {
                    //25 - 25 - 50
                    if (Random.value > 0.5f)
                    {
                        mAttackType = attackTypes.thirdAttack;
                        SpawnMinions();
                    }
                    else
                    {
                        if (Random.value > 0.5)
                        {
                            mAttackType = attackTypes.secondAttack;
                            BulletRingAttack();

                        }
                        else
                        {
                            mAttackType = attackTypes.firstAttack;
                            ThrustHandAttack();
                        }
                    }
                }
                else
                {
                    //besserk mode
                    //25 - 25 - 50
                    if (Random.value > 0.5f)
                    {
                        mAttackType = attackTypes.thirdAttack;
                        SlamAttack();
                    }
                    else
                    {
                        if (Random.value > 0.5)
                        {
                            mAttackType = attackTypes.secondAttack;
                            BulletRingAttack();
                        }
                        else
                        {
                            mAttackType = attackTypes.firstAttack;
                            ThrustHandAttack();
                        }
                    }
                }
            }
        }

        /*
        else if (attackType == AttackType.ranged)
        {
            if (myState == enemyState.attacking && attackCD <= 0 && readyToAttack == true)
            {
                attacking = false;
                readyToAttack = false;
                StartCoroutine(CoolDown());
                //Debug.Log("Bang Bang");
                GameObject bullet = Instantiate(projectile, transform.position, transform.rotation);
                bullet.GetComponent<Bullet>().damageToDeal = baseAttack;

            }
        }
        */
    }

    //attacks
    void ThrustHandAttack()
    {
        Debug.Log("Starting Thrust");
        StartCoroutine(ThrustForward());
    }

   // public bool _currentlyInAttackMovement = false;
    IEnumerator ThrustForward()
    {
        _currentlyInAttackMovement = true;
        //show some indication of about to thrust (maybe color, sound or something)
        readyToAttack = false;
        for (int distance2 = 0; distance2 <= 10; distance2++)
        {
            this.transform.position -= Vector3.forward * Time.deltaTime * wanderSpeed;
        }

            attackCD = attackSpeed;
        yield return new WaitForSeconds(1.5f);
        
        for(int distance = 0; distance <= 7; distance++)
        {
            //causes it to skip forward (kinda cool)
            this.transform.Translate(Vector3.forward);
            
            //or move forward really fast
            /*
             * transform.position(Vector3.forward  * Time.deltaTime * combatSpeed);
             */
            RaycastHit attackRay;

            if (Physics.BoxCast(this.transform.position, Vector3.zero, transform.forward, out attackRay, transform.rotation, attackRange))
            {
                if ((LayerMask.LayerToName(attackRay.transform.gameObject.layer) == "Player" || LayerMask.LayerToName(attackRay.transform.gameObject.layer) == "Wall") && attackCD <= 0 && myState == enemyState.attacking)
                {
                    StartCoroutine(CoolDown());

                    attackRay.transform.GetComponent<Player>().TakeDamage(baseAttack);
                    StopCoroutine(ThrustForward());
                    //Debug.LogError("HitPlayer");
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
        //delay after attack
        yield return new WaitForSeconds(2.0f);
        
        
        StartCoroutine(CoolDown());
    }

    public bool shotGun = false;
    void BulletRingAttack()
    {
        readyToAttack = false;

        if (!shotGun)
        {
            Debug.Log("Ring attack");

            StartCoroutine(BulletRingAttackMovement());
        }
        else
        {
            StartCoroutine(ShotGunBlast());
        }
    }

    public int pelletCountMin = 10;
    public int pelletCountMax = 5;
    public float spreadAngle;
    List<Quaternion> pellets;
    IEnumerator ShotGunBlast()
    {
        int repeat = 1;
        float pauseBetweenShoots = 1.0f;
        if (mbossPhase == bossPhases.phase1)
        {
            repeat = Random.Range(1, 2);
            pauseBetweenShoots = Random.Range(1.0f, 2.0f);
        }
        else
        {
            repeat = Random.Range(2, 3);
            pauseBetweenShoots = Random.Range(0.9f, 1.0f);
        }

        for (int shootsFired = 0; shootsFired < repeat; shootsFired++)
        {
            int pelletNum = Random.Range(pelletCountMin, pelletCountMax);
            pellets = new List<Quaternion>(pelletNum);
            for (int a = 0; a < pelletNum; a++)
            {
                pellets.Add(Quaternion.Euler(Vector3.zero));
            }

            //fire shot
            Debug.Log("PEW");
            //int i = 0;
            for (int c = 0; c < pellets.Count; c++)
            {
                pellets[c] = Random.rotation;
                GameObject bullet = Instantiate(projectile, transform.position, transform.rotation);
                //set variables to bullet
                bullet.transform.rotation = Quaternion.RotateTowards(bullet.transform.rotation, Random.rotation, spreadAngle);
                // bullet.GetComponent<Bullet>().type = type;
                //bullet.GetComponent<Bullet>().damage = damage;
                //bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletVelocity);
                c++;
            }
            if(shootsFired == repeat - 1)
            {
                yield return new WaitForSeconds(pauseBetweenShoots);
            }
        }
        StartCoroutine(CoolDown());
    }

    IEnumerator BulletRingAttackMovement()
    {
        //selects random starting index, this is the one that doesnt shoot
        int indexAvoid = Random.Range(0, bulletRing.Length - 1);
        int ringsFired = 1;
        float pauseBetweenRings = 1.0f;
        //can repeat this multiple times
        //shoots everything else
        if (mbossPhase == bossPhases.phase1)
        {
            ringsFired = Random.Range(1, 4);
            pauseBetweenRings = Random.Range(0.9f, 1.2f);
        }
        else
        {
            ringsFired = Random.Range(2, 5);
            pauseBetweenRings = Random.Range(0.7f, 0.9f);
        }
        
        for(int fired = 0; fired < ringsFired; fired++)
        {
            for(int index = 0; index < bulletRing.Length; index++)
            {
                if (index != indexAvoid)
                {
                    GameObject bullet = Instantiate(projectile, bulletRing[index].transform.position, bulletRing[index].transform.rotation);

                    
                    //bullet.GetComponent<Bullet>().speed = bullet.GetComponent<Bullet>().speed / 2;
                }
            }

            if(fired != ringsFired - 1)
                yield return new WaitForSeconds(pauseBetweenRings);
        }
        

        attackCD = attackSpeed;
        StartCoroutine(CoolDown());
    }

    //bool spawningMinions = false;
    void SpawnMinions()
    {
        //spawningMinions = true;
        readyToAttack = false;
        Debug.Log("Starting spawn");
        //attacking = false;
        
        attackCD = attackSpeed;

        StartCoroutine(CoolDown());
    }

    void SlamAttack()
    {
        readyToAttack = false;
        Debug.Log("Starting slam");
        
        attackCD = attackSpeed;
        StartCoroutine(CoolDown());
    }

    IEnumerator CoolDown()
    {
        
        //reset attack cooldown
        if (mbossPhase == bossPhases.phase1)
        {
            attackSpeed = Random.Range(3, 4);
        }
        else
            attackSpeed = Random.Range(2, 3);

        //Debug.Log("cooling down");
        yield return new WaitForSeconds(attackSpeed);
        //spawningMinions = false;
        //Debug.Log("ready to attack");
        attackCD = 0;
        readyToAttack = true;
        attacking = false;
        _currentlyInAttackMovement = false;
        mAttackType = attackTypes.none;
    }

    /// <summary>
    /// this function will be used to set agro
    /// </summary>
    void SetTarget()
    {
        if (Vector3.Distance(transform.position, _target.gameObject.transform.position) <= detectionRadius)
        {
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
        { poi = wanderPoint; }
        else if (agro)
        { poi = _target.transform.position; }
    }

    /// <summary>
    /// This will be called when the enemy is goes agro
    /// </summary>
    void onAgro()
    {

    }

    float x;
    float z;

    bool start = false;
    void WanderingPoint()
    {


        if (start == false)
        {
            x = Random.Range(-1f, 1f);
            z = Random.Range(-1f, 1f);

            start = true;
        }

        if (Vector3.Distance(wanderPoint, _spawnPoint) >= wanderRadius)
        {
            ChangeDirW();
        }
        wanderPoint += new Vector3(x, 0, z) * 1.4f * Time.deltaTime;
    }
    void ChangeDirW()
    {
        x = -x;
        z = -z;
    }

    public void TakeDamage(int damage)
    {
        baseHealth -= damage;
        StartCoroutine(Damaged());
    }
    IEnumerator Damaged()
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

    void OnDeath()
    {
        //make last array interactable


      //  if (isObjectiveEnemy)
      //  {
            Objectives.Instance.SendCompletedMessage(Condition.KillEnemy);
      //  }
        Destroy(this.gameObject);
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


        //detection
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        

        //Gizmos.color = Color.blue;
        // Gizmos.DrawLine(transform.position, _spawnPoint);


        if (agro)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _target.transform.position);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_target.transform.position, attackDistance);
        }
        else
        {
            //wander
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_spawnPoint, wanderRadius);
        }

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(poi, .1f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(wanderPoint, .2f);



    }
}
