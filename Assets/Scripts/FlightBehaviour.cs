using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightBehaviour : MonoBehaviour
{

    public float acceleration;
    public float topSpeed;
    public float turnSpeed;

    //============================================mJoystick Info===================================================//
    // Grabs the X and Y input from the Joystick script. However, you could change this
    // variable to Input.GetAxis("Horizontal") and Input.GetAxis("Vertical"); and replace
    // all instances of mJoystick for non-mobile platforms.
    public Joystick mJoystick;
    //=============================================================================================================//



    //============================================SphereCast Info===================================================//
    // Casts a sphere downwards to check for ground and prevent collision
    public float sphereCastLength;
    public float sphereCastRadius;
    public LayerMask sphereCastMask;
    private bool amGrounded;
    //=============================================================================================================//


    public Transform myModel; //============ Rotates this gameobject to match the X and Y input
    private Rigidbody rbody; //============= The player's rigidbody

    void Start()
    {
        rbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        AccelerateMeForwards();

        TurnMe();

        SpinModelWhileTurning();
    }

    private void AccelerateMeForwards()
    {
        //Apply force if above a certain threshold
        //remove velocity if below it
        if (acceleration > 0.2f)
            rbody.AddForce(transform.forward * acceleration, ForceMode.Impulse);
        else
            rbody.velocity = Vector3.zero;

        //Clamp the player's velocity to their topSpeed
        rbody.velocity = Vector3.ClampMagnitude(rbody.velocity, topSpeed);
    }

    private void TurnMe()
    {
        // Take the Joystick input and put them into a single vector
        Vector3 locVel = (new Vector3(-(mJoystick.newDir.y * 0.1f), (mJoystick.newDir.x * 0.1f), 0)) * (turnSpeed * 0.02f);

        // Turn the local vectors and apply them to the player's angular velocity
        if (locVel != Vector3.zero)
            rbody.angularVelocity = transform.TransformDirection(locVel);

        // Slow down angular velocity at a smooth rate
        rbody.angularVelocity = Vector3.Lerp(rbody.angularVelocity, Vector3.zero, 0.02f);

         /* The next two lines of code will automatically rotate the player so that their underside will always
         face down. Disabling them will allow the player to fly more freely, meaning they can do inside/outside
         loops easier than if they had auto-orient off. However, the player will not have a way to re-orient
         themselves with the ground afterwards. */

        float downDot = Vector3.Dot(transform.right, Vector3.down) * 1.5f;
        transform.Rotate(transform.InverseTransformDirection(transform.forward * downDot));
    }

    public void HoverOverGround()
    {
        RaycastHit hit;

        // Cast a sphere under the player and push them away to stop collision
        if (Physics.SphereCast(transform.position, sphereCastRadius, (Vector3.down + (transform.forward)).normalized, out hit, sphereCastLength, sphereCastMask) && mJoystick.newDir.y < 25)
        {
            //check if the floor is not actually a wall by making sure it is not close to 90 degrees of Vector3.down
            if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.down)) > 0.1f)
            {
                amGrounded = true;

                // slowly lerp the player to the correct Y position
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, (hit.point + Vector3.up * 2).y, transform.position.z), 0.1f);

                if (transform.position.y < hit.point.y + 1)
                {
                    transform.position = new Vector3(transform.position.x, (hit.point + Vector3.up * 1).y, transform.position.z);
                }
                transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(Quaternion.identity.x, transform.rotation.y, Quaternion.identity.z, transform.rotation.w), 0.1f);
            }
        }
        else
        {
            amGrounded = false;
        }
    }

    public void SpinModelWhileTurning()
    {
        // The player actually stays steady while turning.
        // This script turns the model instead.

        Vector3 stickDirInWorldY = transform.up * mJoystick.newDir.y;
        Vector3 stickDirInWorldX = transform.right * mJoystick.newDir.x;
        Vector3 rayDir = stickDirInWorldY + stickDirInWorldX;

        if (mJoystick.newDir != Vector3.zero)
        {
            float turnDot = Vector3.Dot(rayDir.normalized, myModel.right);

            if (Mathf.Abs(turnDot) > 0.2f)
                myModel.Rotate(0, 0, (mJoystick.newDir.magnitude * 0.1f) * -turnDot, Space.Self);
        }
        else
        {
            float myDownDot = Vector3.Dot(myModel.right, Vector3.down) * 5;
            myModel.Rotate(myModel.InverseTransformDirection(myModel.forward * myDownDot));
        }
    }
}
