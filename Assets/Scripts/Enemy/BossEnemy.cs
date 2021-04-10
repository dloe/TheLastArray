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

    public enemyState myState = enemyState.following;

    // the base health of the enemy
    public int baseHealth = 100;

    //enemys base attack value - changes based on attack
    [Header("attack stats")]
    public int baseAttack = 5;
    public int thrustAttack = 5;
    public int shotgunPelletDamage = 3;
    public int bulletRingDamage = 3;
    public int slamDamage = 5;

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
    float agroLoseDis = 25;

    [Header("if melee")]
    //how far the attack will go
    public float attackRange;

    [Header("if ranged enemy")]
    public GameObject projectile;

    // [Header("Boss apendages")]
    GameObject attack1_hand;
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
        //WanderingPoint();

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

    //public Vector3 delta;
    public float speed = 5;
    //public Quaternion rotMod;
    //public Vector3 dir;
    // public RaycastHit hitInfo;
    void Stearing()
    {
        float speed = 0;
        switch (myState)
        {
            case enemyState.wandering:
                speed = baseSpeed;
                break;
            case enemyState.following:
                speed = baseSpeed;
                break;
            case enemyState.attacking:
                if (mAttackType != attackTypes.thirdAttack)
                    speed = combatSpeed;
                else
                    speed = combatSpeed / 2;
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
        //Debug.Log(delta);

        switch (mAttackType)
        {
            case attackTypes.none:
                //idea: if boss is to close to player, dont move towards it anymore
                //Debug.Log("moving");
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
                if (myState == enemyState.attacking && readyToAttack == true)
                {
                    // this.transform.position += delta * Time.deltaTime;
                    attacking = true;
                }
                break;
            case attackTypes.forthAttack:
                if (myState == enemyState.attacking && readyToAttack == true)
                {
                    // this.transform.position += delta * Time.deltaTime;
                    attacking = true;
                }
                break;
            case attackTypes.thirdAttack:
                if (mbossPhase == bossPhases.phase2)
                {
                    if (myState != enemyState.attacking && readyToAttack == true)
                    {
                        this.transform.position += delta * Time.deltaTime;
                    }
                    else if (myState == enemyState.attacking && readyToAttack == true)
                    {
                        //this.transform.position += delta * Time.deltaTime;
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
                        if (myState == enemyState.attacking)
                            this.transform.position -= delta * Time.deltaTime * 1 / 2;
                    }
                }

                //doesnt move if spawning shit
                break;
            default:
                break;
        }

        if (!_currentlyInAttackMovement)
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
        //randomly pick from 4 attacks
        //if player is in certain distance, have weight  be more for close range
        if (myState == enemyState.attacking && readyToAttack)
        {
            if (mbossPhase == bossPhases.phase1)
            {
                //phase 1 
                if (playerDistanceFromBoss <= 7.5f)
                {
                    //more weight to close range attacks
                    if (mbossPhase == bossPhases.phase1)
                    {
                        //40 - 25 - 25 - 10
                        if (Random.value <= 0.4f)
                        {
                            ThrustHandAttack();
                        }
                        if (Random.value <= 0.5f)
                        {
                            if (Random.value <= 0.5f)
                            {
                                BulletRingAttack();
                            }
                            else
                            {
                                ShootGunBlast();
                            }
                        }
                        else
                        {
                            //spawn stuff
                            SpawnMinions();
                        }
                    }
                }
                else
                {
                    Debug.Log("far");
                    //25 - 15 - 20 - 30
                    if (Random.value <= 0.30f)
                        SpawnMinions();
                    else if (Random.value <= 0.25f)
                        ThrustHandAttack();
                    else if (Random.value <= 0.2f)
                        ShootGunBlast();
                    else
                        BulletRingAttack();
                }
            } 
            else {
                //phase 2
                //super close
                if (playerDistanceFromBoss <= 4f)
                {
                    //if really close then slam heavy
                    //20 0 0 80
                    if (Random.value <= 0.8)
                        SlamAttack();
                    else
                        ThrustHandAttack();
                }
                else if (playerDistanceFromBoss <= 8)
                {
                    //medium distance
                    //besserk mode
                    //20 - 20 - 20 - 40
                    if(Random.value <= 0.4)
                        SlamAttack();
                    else
                    {
                        if (Random.value < 0.33f)
                            ThrustHandAttack();
                        else if (Random.value < 0.33f)
                            BulletRingAttack();
                        else
                            ShootGunBlast();
                    }
                }
                else
                {
                    //far distance (greater than 8)
                    //besserk mode
                    //30 - 25 - 30 - 15
                    if (Random.value <= 0.3f)
                    {
                        if (Random.value >= 0.5)
                            ShootGunBlast();
                        else
                            ThrustHandAttack();
                    }
                    else if (Random.value > 0.25)
                        BulletRingAttack();
                    else
                        SlamAttack();
                }
            }
        }
    }


    //attacks
    void ThrustHandAttack()
    {
        mAttackType = attackTypes.firstAttack;
        Debug.Log("Starting Thrust");
        StartCoroutine(ThrustForward());
    }

   // public bool _currentlyInAttackMovement = false;
    IEnumerator ThrustForward()
    {
        _currentlyInAttackMovement = true;
        //show some indication of about to thrust (maybe color, sound or something)
        readyToAttack = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX;
        for (int distance2 = 0; distance2 <= 12; distance2++)
        {
            //prevent
            Vector3 thrustV = new Vector3(Vector3.forward.x, 0, Vector3.forward.z);
            
            this.transform.position -= thrustV * Time.deltaTime * speed;
            yield return new WaitForSeconds(0.1f);
        }
        
        attackCD = attackSpeed;
        yield return new WaitForSeconds(1.0f);
        
        for(int distance = 0; distance <= 8; distance++)
        {
            //causes it to skip forward (kinda cool)
            this.transform.Translate(Vector3.forward);
           // this.transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
             
            RaycastHit attackRay;

            if (Physics.BoxCast(this.transform.position, Vector3.zero, transform.forward, out attackRay, transform.rotation, attackRange * 2))
            {
                
                if (LayerMask.LayerToName(attackRay.transform.gameObject.layer) == "Player")
                {
                    Debug.Log("HitPlayer");
                    attackRay.transform.GetComponent<Player>().TakeDamage(thrustAttack);
                    yield return new WaitForSeconds(1.5f);
                    StartCoroutine(CoolDown());
                    StopCoroutine(ThrustForward());
                    //yield break; ;
                    break;
                }
                else if(LayerMask.LayerToName(attackRay.transform.gameObject.layer) == "Enviroment")
                {
                    Debug.Log("hit wall stawp");
                    yield return new WaitForSeconds(1.5f);
                    StartCoroutine(CoolDown());
                    StopCoroutine(ThrustForward());
                    //yield break;
                    break;
                } 
            }
            yield return new WaitForSeconds(0.2f);
        }
        //delay after attack
        yield return new WaitForSeconds(1.5f);
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        StartCoroutine(CoolDown());
    }

    public bool shotGun = false;
    void BulletRingAttack()
    {
        mAttackType = attackTypes.secondAttack;
        readyToAttack = false;
        Debug.Log("Ring attack");
        StartCoroutine(BulletRingAttackMovement());
    }

    void ShootGunBlast()
    {
        mAttackType = attackTypes.forthAttack;
        readyToAttack = false;
        Debug.Log("Shutgun");
        StartCoroutine(ShotGunBlast());
    }

    public int pelletCountMin = 3;
    public int pelletCountMax = 5;
    public float spreadAngle = 10;
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
            int pelletNum = 2;
            if (mbossPhase == bossPhases.phase1)
                pelletNum = Random.Range(pelletCountMin, pelletCountMax);
            else
                pelletNum = Random.Range(pelletCountMin + 2, pelletCountMax + 1);

            pellets = new List<Quaternion>(pelletNum);
            for (int a = 0; a < pelletNum; a++)
            {
                pellets.Add(Quaternion.Euler(Vector3.zero));
            }

            //fire shot
            //Debug.Log("PEW");
            //int i = 0;
            for (int c = 0; c < pellets.Count; c++)
            {
                pellets[c] = Random.rotation;
                GameObject bullet = Instantiate(projectile, transform.position, transform.rotation);
                //set variables to bullet
                Quaternion rot = new Quaternion(0, Random.rotation.y, Random.rotation.y, Random.rotation.w);
                bullet.transform.rotation = Quaternion.RotateTowards(bullet.transform.rotation, rot, spreadAngle);
                bullet.transform.parent = GameObject.Find("TileGen").transform;
                //change this attack stat later or balancing
                bullet.GetComponent<Bullet>().damageToDeal = shotgunPelletDamage;
                c++;
            }
            if(shootsFired != repeat - 1)
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
        Debug.Log(indexAvoid);
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

                    //change this damage value later for balancing
                    bullet.GetComponent<Bullet>().damageToDeal = bulletRingDamage;
                    bullet.transform.parent = GameObject.Find("TileGen").transform;
                    //bullet.transform.parent = this.transform;
                    //bullet.GetComponent<Bullet>().speed = bullet.GetComponent<Bullet>().speed / 2;
                }
            }

            if(fired != ringsFired - 1)
                yield return new WaitForSeconds(pauseBetweenRings);
        }
        

        //attackCD = attackSpeed;
        StartCoroutine(CoolDown());
    }

    //bool spawningMinions = false;
    void SpawnMinions()
    {
        mAttackType = attackTypes.thirdAttack;
        //spawningMinions = true;
        readyToAttack = false;
        Debug.Log("Starting spawn");
        //attacking = false;
        
        attackCD = attackSpeed;

        StartCoroutine(CoolDown());
    }

    void SlamAttack()
    {
        mAttackType = attackTypes.thirdAttack;
        readyToAttack = false;
        Debug.Log("Starting slam");

        StartCoroutine(SlamAttackMovement());
    }

    public int slamDistance = 7;
    IEnumerator SlamAttackMovement()
    {
        yield return new WaitForSeconds(1.0f);

        int liftHeight = 30;
        //raise up boss, then drop them causing area of effect
        for(int lift = 0; lift < liftHeight; lift++)
        {
            this.transform.position += Vector3.up * Time.deltaTime * speed * 2;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(1f);
        for(int drop = 0; drop < liftHeight/3; drop++)
        {
            this.transform.position += -Vector3.up * Time.deltaTime * speed * 6;
            yield return new WaitForSeconds(0.05f);
        }

        if(playerDistanceFromBoss < slamDistance)
        {

            //find multiplier based on how close player is
            float multiplier = playerDistanceFromBoss / slamDistance;
            Debug.Log(multiplier);

            //if in range takes damage based on proximity and pushed away
            Debug.Log("Player in range");
            transform.LookAt(_target.transform.position);

            //push target based on how close they are to boss
            _target.GetComponent<Rigidbody>().AddForce(this.transform.forward * multiplier * 1000);
            _target.GetComponent<Rigidbody>().AddForce(Vector3.up * multiplier * 1000);

            //to prevent less than 1 damage being applied but not really applied weird zone
            if (playerDistanceFromBoss <= slamDistance - 1.5f)
            {
                Debug.Log((int)((1 / multiplier) * slamDamage));
                _target.GetComponent<Player>().TakeDamage((int)(1 / multiplier * slamDamage));
            }
        }
        yield return new WaitForSeconds(2f);
        //attackCD = attackSpeed;
        StartCoroutine(CoolDown());
    }

    IEnumerator CoolDown()
    {
        attackSpeed = 2;
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
        //Gizmos.DrawCube(transform.position + Vector3.forward, )


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
          //  Gizmos.color = Color.blue;
          //  Gizmos.DrawWireSphere(_spawnPoint, wanderRadius);
        }

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(poi, .1f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(wanderPoint, .2f);



    }
}
