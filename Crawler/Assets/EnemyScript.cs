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
    [SerializeField] EnemyState State= EnemyState.Wondering;
    [SerializeField] EnemyState PreNoticeState = EnemyState.Wondering;


    [SerializeField] Animator Animator;


    public bool canSeePlayer;
    public Transform CurrentlyChosenRoom;

    public Vector3 PlayerLastSeenPosition;

    [SerializeField] float CloseEnoughToRoom = 3;

    [SerializeField] float TimeToForgetPlayer = 0;
    [SerializeField] float SetTimeToForgetPlayer = 3;

    [SerializeField] float ChangeRoomDestinationTimeout = 30;

    float LastRoomChangeTimeStamp;
    [SerializeField] float IdleTimeLeft =0;
    private void Start()
    {
        Animator = GetComponent<Animator>();
        playerRef = GameObject.FindGameObjectWithTag("Player");

        SetRandomRoom();
    StartCoroutine(FOVRoutine());
      //  navmesh.isStopped = true; //Optymizacja, po prostu zostaw to tak
    }
    void SetRandomRoom()
    {
        int randomID = UnityEngine.Random.Range(0, RoomManager.instance.Rooms.Count - 1);
        Debug.Log("Random Room choes by the enemy is: " + randomID);
        CurrentlyChosenRoom = RoomManager.instance.Rooms[randomID];
        LastRoomChangeTimeStamp = Time.time;
    }
    bool SawPlayerBefore = false;



    private void Update()
    {
        bool playerDiscovered = false;
        bool playerLost = false;

        TimeToForgetPlayer -= Time.deltaTime;
        if (canSeePlayer) TimeToForgetPlayer = SetTimeToForgetPlayer;

        if (canSeePlayer != SawPlayerBefore)
        {
            if (canSeePlayer)
                playerDiscovered = true;
            else
            {
                playerLost = true;
                PlayerLastSeenPosition = playerRef.transform.position;
            }
        }
       if (playerDiscovered)
        { 
            PreNoticeState = State;
            State = EnemyState.Chacing;        
        }
        if (playerLost)
            {
            State = PreNoticeState;
            }
    
        SawPlayerBefore = canSeePlayer;
        
        ManageState();

    }
    void ManageState()
    {
        switch (State)
        {
            case EnemyState.Idle:

                Animator.SetBool("isRunning", false);
                if(IdleTimeLeft<=0)
                {
                    SetRandomRoom();

                    State = EnemyState.Wondering;
                    PreNoticeState = EnemyState.Wondering;

                }
                else
                {
                
                    IdleTimeLeft -= Time.deltaTime;
                }

                break;
            case EnemyState.Wondering:
                Animator.SetBool("isRunning", true);

                navmesh.SetDestination(CurrentlyChosenRoom.transform.position);
                if(navmesh.isStopped)
                {
                    navmesh.isStopped = false;
                }
               // Debug.Log("Distance to chosen room : " + Vector3.Distance(transform.position, CurrentlyChosenRoom.transform.position));
                if(Vector3.Distance(transform.position, CurrentlyChosenRoom.transform.position) < CloseEnoughToRoom 
                    || (Time.time - LastRoomChangeTimeStamp> ChangeRoomDestinationTimeout) )
                {
                    State = EnemyState.Idle;
                    PreNoticeState = EnemyState.Idle;
                    IdleTimeLeft = UnityEngine.Random.Range(1f, 2f);
                    navmesh.isStopped = true;

                }

                
                if(TimeToForgetPlayer>0)
                    navmesh.SetDestination(playerRef.transform.position);
                    break;
            case EnemyState.Notice:


                break;
            case EnemyState.Chacing:

                navmesh.SetDestination(playerRef.transform.position);



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

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") )
        {
            PlayerMovement playerMoveScript = playerRef.GetComponent<PlayerMovement>();
            //  if (playerMoveScript.Dead) return;

            playerMoveScript.PlayJumpscare();
            Destroy(this.gameObject);
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
