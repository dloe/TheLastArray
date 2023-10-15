using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionAirDropBehavior : MonoBehaviour
{
    /// <summary>
    /// Minion Airdrop behavior
    /// Dylan Loe
    /// Updated: 4-12
    /// 
    /// - Minions can spawn in the boss room
    /// - special behavior needed to add cool entrance
    ///     - Sets line renderer initial values
    /// </summary>
    public int damage;
    public float baseMeleeKnockback = 7f;
    public float speed;
    public float checkRadius = 2;
    Vector3 _rayHitTrans;

    LineRenderer _airdropLine;

    public float lineSpeed = 1.0f;
    float _distance;
    float _increment;

    public GameObject minion;

    void Start()
    {
        _airdropLine = GetComponent<LineRenderer>();
        _airdropLine.positionCount = 2;
        _airdropLine.startWidth = 0.45f;
        _airdropLine.SetPosition(0, this.transform.position);
        _airdropLine.SetPosition(1, this.transform.position);
        CastLine();
    }

    /// <summary>
    /// Dylan Loe
    /// Updated: 4-12
    /// 
    /// Moves downward 
    /// </summary>
    void FixedUpdate()
    {
        transform.position += speed * Time.deltaTime * -this.transform.up;
    }

    /// <summary>
    /// Dylan Loe
    /// Updated: 4-12
    /// 
    /// Animate line as it moves downward
    /// </summary>
    private void Update()
    {
        if (_increment < _distance)
        {
            _increment += .1f / lineSpeed;

            float x = Mathf.Lerp(0, _distance, _increment);

            Vector3 pointA = this.transform.position;
            Vector3 pointB = _rayHitTrans;

            Vector3 alongLine = x * Vector3.Normalize(pointB - pointA) + pointA;
            _airdropLine.SetPosition(0, this.transform.position);
            _airdropLine.SetPosition(1, alongLine);
        }
    }

    /// <summary>
    /// Dylan Loe
    /// Updated: 4-12
    /// 
    /// If it hits anything
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        //if it hits anything
        if (other)
            Detonation();
    }

    bool _check = false;
    /// <summary>
    /// Dylan Loe
    /// Updated: 4-12
    /// 
    /// Looks for player in detection then damages them
    /// </summary>
    void Detonation()
    {
        //run particle effect
        Debug.Log("Detonation: HIT FLOOR");
        //damage player
        Transform[] _nearby = collidersToTransforms(Physics.OverlapSphere(transform.position, checkRadius));
        foreach (Transform potentialTarget in _nearby)
        {
            if (potentialTarget.gameObject.tag == "Player")
            {
                //player in range, damage player
                potentialTarget.gameObject.GetComponent<Player>().TakeDamage(1, baseMeleeKnockback, this.transform);
            }
        }

        //spawn minion
        //Debug.Log("Spawn minion");
        //set y pos to 0.5f in case detonation takes place to soon
        if (!_check)
        {
            _check = true;
            Instantiate(minion, transform.position, transform.rotation);
        }

        Destroy(this.gameObject);
    }


    /// <summary>
    /// Dylan Loe
    /// Updated: 4-12
    /// 
    /// Starts line detection based on if there is ground under them. Mortar will cast a raycast down until it hits the ground. Line renderer will go from this.transform.position to the hit.transform.position
    /// </summary>
    void CastLine()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            _airdropLine.enabled = true;
            _rayHitTrans = hit.point;
            _distance = Vector3.Distance(this.transform.position, _rayHitTrans);
        }
    }

    /// <summary>
    /// Dylan Loe
    /// Updated: 4-12
    /// 
    /// takes colliders into transforms
    /// </summary>
    private Transform[] collidersToTransforms(Collider[] colliders)
    {
        Transform[] transforms = new Transform[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            transforms[i] = colliders[i].transform;
        }
        return transforms;
    }

    /// <summary>
    /// Dylan Loe
    /// Uodated: 4-12
    /// 
    /// Show gizmos in scenen
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}
