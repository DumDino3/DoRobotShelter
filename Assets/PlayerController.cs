
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("REFERENCES")]
    public PhysicsCalculator physicsCalculator;
    public RaycastDetector raycastDetector;
    public PivotManager pivotManager;

    [Header("MOVEMENT SETTINGS")]
    public float moveSpeed;
    public float gravityStrength;
    public float airMoveValue;
    public float jumpStrength;

    [Header("RUNTIME STATE")]
    public float moveDir;
    public Vector3 velocity;
    public GameObject Body;
    public MovementState currentState = MovementState.Grounded;

    [Header("COLLISION CHECKER")]
    public bool wallLeftHit = false;
    public bool wallRightHit = false;
    public bool ceilingHit = false;
    public bool isGrounded = true;

    [Header("PIVOT VARIABLES")]
    public float ropeLength;
    public float speed;
    public float angularVelocity;
    private float swingAngle;
    private Vector3 pivotPosition;

    [Header("PROCEDURAL VARIABLES")]
    private bool isRight = true;


    //------------------------------------------------------------------------------------------------- ON UPDATE -------------------------------------------------------------------------------------------------

    void Update()
    {
        ReadInput();

        RotateBodyFoward();

        Debug.Log(currentState);

        DetectPivotIfAirborne();

        //Pivot physics
        if (currentState == MovementState.Pivot)
        {
            Vector3 newPos = physicsCalculator.CalculateVelocity(
                currentState,
                velocity,
                moveSpeed,
                moveDir,
                airMoveValue,
                gravityStrength,
                jumpStrength,
                ref swingAngle,
                ref angularVelocity,
                pivotPosition,
                ropeLength,
                Time.deltaTime);
            transform.position = newPos;
            //velocity = Vector3.zero;
        }
        //Normal movement physics
        else
        {
            velocity = physicsCalculator.CalculateVelocity(
                currentState,
                velocity,
                moveSpeed,
                moveDir,
                airMoveValue,
                gravityStrength,
                jumpStrength,
                ref swingAngle,
                ref angularVelocity,
                pivotPosition,
                ropeLength,
                Time.deltaTime);
            CollisionCheck();
            ApplyVelocity();
        }
    }

    //------------------------------------------------------------------------------------------------- INPUT -------------------------------------------------------------------------------------------------
    void ReadInput() //This has been put in update
    {
        // Basic horizontal input
        moveDir = 0;
        if (Input.GetKey(KeyCode.A)) moveDir = -1;
        if (Input.GetKey(KeyCode.D)) moveDir = 1;

        //Space input
        if (Input.GetKeyDown(KeyCode.Space) && currentState == MovementState.Grounded && isGrounded) //Grounded -> Airborne jump
        {
            velocity.y = jumpStrength;
            EnterAirborne(); // force airborne
        }

        //Enable pivot attach if airborne
        if (pivotManager.currentPivot != null && Input.GetKeyDown(KeyCode.J))
        {
            AttachToPivot(pivotManager.currentPivot);
        }

        // Pivot release
        if (currentState == MovementState.Pivot && Input.GetKeyUp(KeyCode.J))
        {
            //Debug.Log("unpressed J");
            DetachFromPivot();
        }
    }

    //------------------------------------------------------------------------------------------------- PHYSICS -------------------------------------------------------------------------------------------------

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

        //Grounding logic
        if (!isGrounded && currentState != MovementState.Airborne && currentState != MovementState.Pivot)
        {
            EnterAirborne();
        }
        else if (isGrounded && currentState == MovementState.Airborne && velocity.y <= 0)
        {
            EnterGrounded();
        }

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
    }

    //------------------------------------------------------------------------------------------------- STATE LOGIC -------------------------------------------------------------------------------------------------
    void EnterAirborne()
    {
        isGrounded = false;
        currentState = MovementState.Airborne;
    }

    void EnterGrounded()
    {
        currentState = MovementState.Grounded;
        velocity.y = 0f;   // reset vertical velocity
    }

    void DetectPivotIfAirborne()
    {
        if (currentState == MovementState.Airborne)
        {
            pivotManager.DetectClosestPivot(transform.position);
        }
    }

    void AttachToPivot(Transform pivot)
    {
        pivotPosition = pivot.position;

        // Calculate rope length
        //ropeLength = Vector3.Distance(transform.position, pivotPosition);

        // Calculate angle relative to vertical down
        Vector3 dir = (transform.position - pivotPosition).normalized;
        swingAngle = Mathf.Atan2(dir.x, -dir.y);

        //  Give initial angular velocity based on horizontal movement
        // So if you attach mid-run, you swing immediately
        //float baseAngular = velocity.x / Mathf.Max(ropeLength, 0.01f);
        //angularVelocity = Mathf.Sign(baseAngular) * Mathf.Max(Mathf.Abs(baseAngular) * 1.5f, 2.3f);

        //float baseAngular = Mathf.Abs(velocity.x) / velocity.x;
        float baseAngular = 1;
        if (isRight) { baseAngular = 1; } else if (!isRight) { baseAngular = -1; }

        angularVelocity = baseAngular * speed;

        //  SNAP player exactly onto the rope arc
        Vector3 offset = new Vector3(Mathf.Sin(swingAngle), -Mathf.Cos(swingAngle), 0f) * ropeLength;
        transform.position = pivotPosition + offset;

        currentState = MovementState.Pivot;

        //Debug.Log($"AttachToPivot: {pivot.name}, ropeLength={ropeLength}, swingAngle={swingAngle}, angularVel={angularVelocity}");
    }

    void DetachFromPivot()
    {
        // Compute fling tangent velocity
        Vector3 tangentDir = new Vector3(Mathf.Cos(swingAngle), Mathf.Sin(swingAngle), 0f);
        velocity = tangentDir * (angularVelocity * ropeLength);

        currentState = MovementState.Airborne;
    }

    //------------------------------------------------------------------------------------------------- PROCEDURAL LOGIC -------------------------------------------------------------------------------------------------
    void RotateBodyFoward()
    {
        if (moveDir > 0)
        {
            Vector3 localEuler = Body.transform.localEulerAngles;
            localEuler.y = -6.445f;
            Body.transform.localEulerAngles = localEuler;

            isRight = true;
        }

        else if (moveDir < 0)
        {
            Vector3 localEuler = Body.transform.localEulerAngles;
            localEuler.y = -6.445f + 180f;
            Body.transform.localEulerAngles = localEuler;

            isRight = false;
        }
    }
}

//------------------------------------------------------------------------------------------------- STATES ENUM -------------------------------------------------------------------------------------------------
public enum MovementState
{
    Grounded,
    Airborne,
    Pivot
}