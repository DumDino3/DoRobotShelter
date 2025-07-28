using UnityEngine;

[System.Serializable]
public class PhysicsCalculator
{
    public Vector3 CalculateVelocity(
        MovementState state,
        Vector3 currentVelocity,
        float moveSpeed,
        float moveDir,
        float queuedDir,
        float airMoveValue,
        float gravityStrength,
        float jumpStrength,
        float swingAngle,
        float angularVelocity,
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
                currentVelocity.y = 0f; // No vertical movement
                break;

            //------------------------------------------------------------------------------------- AIRBORNE ----------------------------------------------------------------------------------------------
            case MovementState.Airborne:
                // Burst in one direction
                //if (queuedDir != 0)
                //{
                //    currentVelocity.x = queuedDir * moveSpeed;
                //    // queuedDir cleared automatically in PlayerController after 1 frame
                //}

                // --- Continuous Airborne horizontal control ---
                // Smoothly lerp toward new input in air
                float targetAirSpeed = moveDir * moveSpeed;
                currentVelocity.x = Mathf.Lerp(currentVelocity.x, targetAirSpeed, airMoveValue * deltaTime);

                currentVelocity.y += gravityStrength * deltaTime; //Gravity

                break;

            //--------------------------------------------------------------------------------------- PIVOT ----------------------------------------------------------------------------------------------
            case MovementState.Pivot:
                // Placeholder for later (swing mechanics)
                break;
        }

        return currentVelocity;
    }
}