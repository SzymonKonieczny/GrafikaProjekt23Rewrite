using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using UnityEngine.AI;
 enum EnemyState
    {
        Idle,
        Wondering,
        Notice,
        Chacing
    }
public class EnemyScript : MonoBehaviour
{
   
    public float radius;
    [Range(0, 360)]
    public float angle;
    public List<AudioClip> audioClips = new List<AudioClip>();
    int currentlyPlayingSound;
    public AudioSource audioSource;
    public GameObject playerRef;
    public NavMeshAgent  navmesh;
    public LayerMask targetMask;
    public LayerMask obstructionMask;
    [SerializeField] EnemyState State= EnemyState.Idle;
    public bool canSeePlayer;



    float IdleTimeLeft=5;
    private void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVRoutine());
    }

    private void Update()
    {
        if (canSeePlayer)
        {
            navmesh.SetDestination(playerRef.transform.position);
        }
        ManageState();

    }
    void ManageState()
    {
        switch (State)
        {
            case EnemyState.Idle:


                break;
            case EnemyState.Wondering:



                break;
            case EnemyState.Notice:


                break;
            case EnemyState.Chacing:



                break;
        }

    }

    void PickSoundToPlay()
    {

    }
    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.02f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }


    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    canSeePlayer = true;
                else
                    canSeePlayer = false;
            }
            else
                canSeePlayer = false;
        }
        else if (canSeePlayer)
            canSeePlayer = false;
    }
}
