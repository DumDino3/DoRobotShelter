using UnityEngine;

[System.Serializable]
public class PhysicsCalculator
{
    public Vector3 CalculateVelocity(
        MovementState state,
        Vector3 currentVelocity,
        float moveSpeed,
        float moveDir,
        float airMoveValue,
        float gravityStrength,
        float jumpStrength,
        ref float swingAngle,
        ref float angularVelocity,
        Vector3 pivotPosition,
        float ropeLength,
        float deltaTime
    )
    {
        switch (state)
        {
            //------------------------------------------------------------------------------------- GROUNDED ----------------------------------------------------------------------------------------------
            case MovementState.Grounded:
                currentVelocity.x = moveDir * moveSpeed;
                currentVelocity.y = 0f;
                return currentVelocity;

            //------------------------------------------------------------------------------------- AIRBORNE ----------------------------------------------------------------------------------------------
            case MovementState.Airborne:
                float targetAirSpeed = moveDir * airMoveValue;
                currentVelocity.x = Mathf.Lerp(currentVelocity.x, targetAirSpeed, 0.1f);
                // Gravity
                currentVelocity.y -= gravityStrength * deltaTime;
                return currentVelocity;

            //--------------------------------------------------------------------------------------- PIVOT ----------------------------------------------------------------------------------------------
            case MovementState.Pivot:

                float gravity = 9.81f;

                // Pendulum angular acceleration
                float angularAcceleration = -(gravity / ropeLength) * Mathf.Sin(swingAngle);

                // Update angular velocity + angle
                angularVelocity += angularAcceleration * deltaTime;
                swingAngle += angularVelocity * deltaTime;

                // Constrained offset from pivot
                Vector3 offset = new Vector3(
                    Mathf.Sin(swingAngle),
                    -Mathf.Cos(swingAngle),
                    0f
                ) * ropeLength;

                // Return absolute new position
                return pivotPosition + offset;

            default:
                return currentVelocity;
        }
    }
}
