using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private const float LANE_DISTANCE = 2.5f;
    private const float TURN_SPEED = 0.05f;

    private bool isRunning = false;

    private Animator anim;

    private CharacterController controller;
    private float jumpForce = 4.5f;
    private float gravity = 12.0f;
    private float verticalVelocity;

    //speed variables
    //private float originalSpeed = 7.0f;
    private float speed = 7.0f;
    private float speedIncreaseLastTick;
    private float speedIncreaseTime = 2.5f;
    private float speedIncreaseAmount = 0.1f;

    private int desiredLane = 1; //0 = left, 1 = middle and 2 = right

    //player's color
    public bool canStart = false;
    private Color playerColor;
    public Renderer playerRenderer;
    private Color[] possibleColors = { Color.white, Color.green, Color.red };
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        playerColor = possibleColors[PlayerPrefs.GetInt("colorIndex", 0)];
        playerRenderer.GetComponentInChildren<Renderer>().material.color = playerColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRunning)
        {
            return;
        }

        if ( Time.time - speedIncreaseLastTick > speedIncreaseTime )
        {
            speedIncreaseLastTick = Time.time;
            speed += speedIncreaseAmount;
            //change modifier text
            GameManager.Instance.UpdateModifier(speed);
        }

        //get input on which lane we should be
        if (MobileInput.Instance.SwipeLeft )
        {
            moveLane(false);
        }
        if (MobileInput.Instance.SwipeRight)
        {
            moveLane(true);
        }

        //calculate where we should be in the future
        Vector3 targetPosition = transform.position.z * Vector3.forward;
        if(desiredLane == 0 )
        {
            targetPosition += Vector3.left * LANE_DISTANCE;
        }
        else if(desiredLane == 2)
        {
            targetPosition += Vector3.right * LANE_DISTANCE;
        }

        //let's calculate our move delta
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - transform.position).normalized.x * speed;

        //calculate y
        bool isGrounded = IsGrounded();

        anim.SetBool("IsGrounded", isGrounded);
        if (isGrounded)//if grounded
        {
            verticalVelocity = -0.1f;

            if (MobileInput.Instance.SwipeUp)
            {
                //jump
                anim.SetTrigger("Jump");
                verticalVelocity = jumpForce;
            }
            else if (MobileInput.Instance.SwipeDown)
            {
                //slide
                StartSliding();
                Invoke("StopSliding", 1.0f);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
            
            //fast fall mechanic
            if(MobileInput.Instance.SwipeDown)
            {
                verticalVelocity = -jumpForce;
            }
        }

        moveVector.y = verticalVelocity;
        moveVector.z = speed;

        //move the pengu
        controller.Move(moveVector * Time.deltaTime);

        //Rotate the pengu to where he is going
        Vector3 dir = controller.velocity;
        if( dir != Vector3.zero )
        {
            dir.y = 0;
            transform.forward = Vector3.Lerp(transform.position, dir, TURN_SPEED);
        }
    }

    private bool IsGrounded()
    {
        Ray groundRay = new Ray(new Vector3(controller.bounds.center.x,
            (controller.bounds.center.y - controller.bounds.extents.y) + 0.2f,
            controller.bounds.center.z), Vector3.down);
        Debug.DrawRay(groundRay.origin, groundRay.direction, Color.cyan, 1.0f);

        return Physics.Raycast(groundRay, 0.3f);
    }
    private void moveLane(bool goingRight)
    {
        desiredLane += goingRight ? 1 : -1;
        desiredLane = Mathf.Clamp(desiredLane, 0, 2);
    }

    public void StartRunning()
    {
        isRunning = true;
        anim.SetTrigger("StartRunning");
    }

    private void StartSliding()
    {
        anim.SetTrigger("Slide");
        controller.height /= 2;
        controller.center = new Vector3( controller.center.x, controller.center.y /2 , controller.center.z );
    }
    public void StopSliding()
    {
        anim.SetTrigger("Running");
        controller.height *= 2;
        controller.center = new Vector3(controller.center.x, controller.center.y * 2, controller.center.z);
    }

    private void Crash()
    {
        anim.SetTrigger("Death");
        isRunning = false;
        GameManager.Instance.IsDead = true;
        GameManager.Instance.onDeath();
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch (hit.gameObject.tag)
        {
            case "Obstacle":
                Crash();
                break;
        }
    }

    public void changeColor(int colorIndex)
    {
        playerColor = possibleColors[colorIndex];
        PlayerPrefs.SetInt("colorIndex", colorIndex);
        playerRenderer.GetComponentInChildren<Renderer>().material.color = playerColor;
        Debug.Log("changeColor called " + possibleColors[colorIndex]);
        canStart = true;
    }
}