
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joystick : MonoBehaviour
{
    public float speed = 5.0f;
    private float baseSpeed = 5.0f;
    public bool isTouching = false;
    public bool touchStart = false;
    public bool touchLifted = false;
    public float heldTouchTimer = 0;
    public float heldTouchThreshold = 0.5f;
    public bool isHoldingDown = false;
    public float distanceClamp;
    private Vector2 pointA;
    private Vector2 pointB;
    public Vector3 newDir = Vector3.zero;
    public Vector3 unclampedDir = Vector3.zero;
    public float xTouchPos;
    public Touch leftTouch;
    public Touch rightTouch;

    public Transform circle;
    public Transform outerCircle;

    public Transform rightCircle;
    public Transform topThresh;
    public Transform botThresh;

    public FlightBehaviour mFlightBehave;
    public Canvas myCanvas;

    public float speedRatio;


    private void Start()
    {
        baseSpeed = mFlightBehave.acceleration;
        RestartSpeedToStick();
    }

    // Update is called once per frame
    void Update()
    {
        //============================================TOUCH

        if (Input.touchCount > 0)
        {
            foreach (Touch touches in Input.touches)
            {
                if (touches.position.x < Screen.width / 2)
                {
                    leftTouch = touches;

                    
                    xTouchPos = leftTouch.position.x;

                    // Handle finger movements based on TouchPhase
                    switch (leftTouch.phase)
                    {
                        //When a touch has first been detected, change the message and record the starting position
                        case TouchPhase.Began:
                            // Record initial touch position.
                            FirstTouch();
                            break;

                        //Determine if the touch is a moving touch
                        case TouchPhase.Moved:
                            // Determine direction by comparing the current touch position with the initial one
                            LingeringTouch();
                            break;

                        case TouchPhase.Ended:
                            // Report that the touch has ended when it ends
                            Debug.Log("touch phase has ended");
                            TouchEnd();
                            break;
                    }

                }
            }

        }
        //============================================END TOUCH
        
        if (isTouching)
        {
            Vector2 offset = pointB - pointA;
            Vector2 direction = Vector3.ClampMagnitude(offset, distanceClamp);
            moveCharacter(direction, offset);

            circle.localPosition = new Vector2(pointA.x + direction.x, pointA.y + direction.y);
            circle.gameObject.SetActive(true);
            outerCircle.gameObject.SetActive(true);
        }
        else
        {
            circle.gameObject.SetActive(false);
            outerCircle.gameObject.SetActive(false);
            newDir = Vector3.zero;
            unclampedDir = Vector3.zero;
        }

    }

    public void RightStick()
    {
        foreach (Touch rightTouch in Input.touches)
        {
            if (rightTouch.position.x > Screen.width / 2)
            {
                rightCircle.transform.position = rightTouch.position;
                rightCircle.position = new Vector3(botThresh.position.x, Mathf.Clamp(rightCircle.position.y, botThresh.position.y, topThresh.position.y), 0);
                speedRatio = mFlightBehave.acceleration = baseSpeed * ((rightCircle.position.y - botThresh.position.y) / (topThresh.position.y - botThresh.position.y));
                speedRatio /= baseSpeed;
                if (speedRatio < 0)
                    speedRatio = 0;
            }
        }
    }

    public void RestartSpeedToStick()
    {
        speedRatio = mFlightBehave.acceleration = baseSpeed * ((rightCircle.position.y - botThresh.position.y) / (topThresh.position.y - botThresh.position.y));
        speedRatio /= baseSpeed;
        if (speedRatio < 0)
            speedRatio = 0;
    }

    void moveCharacter(Vector2 direction, Vector2 unclampedDirection)
    {
        newDir = new Vector3(direction.x, direction.y, 0);
        unclampedDir = new Vector3(unclampedDirection.x, 0, unclampedDirection.y);
    }

    public void FirstTouch()
    {
            touchStart = true;
            isHoldingDown = false;
            heldTouchTimer = 0; // Reset the time for checking if the player is holding the touch or just tapping

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                myCanvas.transform as RectTransform,
                leftTouch.position, myCanvas.worldCamera,
                out pointA);


            circle.localPosition = pointA;
            outerCircle.localPosition = pointA;
            circle.gameObject.SetActive(true);
            outerCircle.gameObject.SetActive(true);
    }

    public void LingeringTouch()
    {
            touchStart = false;
            isTouching = true;
            if (heldTouchTimer < heldTouchThreshold)
                heldTouchTimer += Time.deltaTime; // add to the time for checking if the player is holding the touch or just tapping
            else
                isHoldingDown = true;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
            myCanvas.transform as RectTransform,
            leftTouch.position, myCanvas.worldCamera,
            out pointB);
    }

    public void TouchEnd()
    {
        isTouching = false;
        isHoldingDown = false;
    }


}