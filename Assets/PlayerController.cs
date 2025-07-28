using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("REFERENCES")]
    public PhysicsCalculator physicsCalculator;
    public RaycastDetector raycastDetector;
    public PivotManager pivotManager;

    [Header("MOVEMENT SETTINGS")]
    public float moveSpeed = 5f;
    public float gravityStrength = -20f;
    public float airMoveValue = 0.1f;
    public float jumpStrength = 0.1f;

    [Header("RUNTIME STATE")]
    public float moveDir;
    public float queuedDir; // saved direction at jump
    public Vector3 velocity;
    public MovementState currentState = MovementState.Grounded;

    public bool wallLeftHit = false;
    public bool wallRightHit = false;
    public bool ceilingHit = false;
    public bool isGrounded = true;

    private Transform pivotTarget;
    private Vector3 pivotPosition;
    private float ropeLength;
    private float swingAngle;
    private float angularVelocity;


    //------------------------------------------------------------------------------------------------- ON UPDATE -------------------------------------------------------------------------------------------------

    void Update()
    {
        ReadInput();

        // Calculate velocity using PhysicsCalculator
        velocity = physicsCalculator.CalculateVelocity(
            currentState,
            velocity,
            moveSpeed,
            moveDir,
            queuedDir,
            airMoveValue,
            gravityStrength,
            jumpStrength,
            ref swingAngle,
            ref angularVelocity,
            pivotPosition,
            ropeLength,
            Time.deltaTime
        );
        //ClearQueuedDir();

        CollisionCheck();
        //if (currentState == MovementState.Airborne && queuedDir != 0)
        //{
        //    queuedDir = 0; // clear after first airborne calculation
        //}
        ApplyVelocity();

        if (currentState == MovementState.Airborne)
            pivotManager.DetectClosestPivot(transform.position);
    }


    //------------------------------------------------------------------------------------------------- HEHE -------------------------------------------------------------------------------------------------

    void ReadInput() //This has been put in update
    {
        // Basic horizontal input
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.D)) input = +1f;
        moveDir = input;

        //Space input
        if (Input.GetKeyDown(KeyCode.Space) && currentState == MovementState.Grounded && isGrounded) //Grounded -> Airborne jump
        {
            velocity.y = jumpStrength;
            EnterAirborne(); // force airborne
        }

        //J input
        if (currentState == MovementState.Airborne &&
            pivotManager.currentPivot != null &&
            Input.GetKeyDown(KeyCode.J))
        {
            AttachToPivot(pivotManager.currentPivotCandidate);
        }

        //J input
        if (currentState == MovementState.Pivot && Input.GetKeyUp(KeyCode.J))
        {
            DetachFromPivot();
        }
    }

    void ApplyVelocity()
    {
        // Always lock Z = 0 so player stays on 2.5D plane
        Vector3 newPos = transform.position + velocity * Time.deltaTime;
        newPos.z = 0f;
        transform.position = newPos;
    }

    void CollisionCheck()
    {
        raycastDetector.CheckEnvironment();
        //Ceiling
        if (ceilingHit && velocity.y > 0)
        {
            velocity.y = 0;
        }

        //Walls 
        if (wallLeftHit && velocity.x < 0)
        {
            velocity.x = 0;
        }
        if (wallRightHit && velocity.x > 0)
        {
            velocity.x = 0;
        }

        //Grounding logic
        if (!isGrounded && currentState != MovementState.Airborne)
        {
            EnterAirborne();
        }
        else if (isGrounded && currentState == MovementState.Airborne && velocity.y <= 0)
        {
            EnterGrounded();
        }
    }

    //void ClearQueuedDir()
    //{
    //    if (currentState == MovementState.Airborne && queuedDir != 0)
    //    {
    //        queuedDir = 0; //clear after first airborne calculation
    //    }
    //}

    //------------------------------------------------------------------------------------------------- STATE ENTER LOGIC -------------------------------------------------------------------------------------------------
    void EnterAirborne()
    {
        queuedDir = moveDir;      // save current horizontal intent
        isGrounded = false;
        currentState = MovementState.Airborne;
    }

    void EnterGrounded()
    {
        currentState = MovementState.Grounded;
        velocity.y = 0f;   // reset vertical velocity
    }
}

//------------------------------------------------------------------------------------------------- STATES ENUM -------------------------------------------------------------------------------------------------
// Enum for states
public enum MovementState
{
    Grounded,
    Airborne,
    Pivot
}