﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class FixedPlayerMovement : MonoBehaviour
{

    //jumping Variables
    float walkJumpVel = 14f;
    float runJumpVel = 18f;
    float fallMultiplier = 8f;
    float lowJumpMultiplier = 8f;
    float bonusGravityMult = 2.5f;
    float airDrag = 0.95f;
    Vector3 bonusGravity;
    public float distToGround = 2f;
    public bool canDoubleJump;


    //moving Variables
    float walkSpeed = 7f;
    float runAccel = 4f;
    float friction = 0.75f;
    float xVel;
    float zVel;
    float airAccel = 1f;
    float maxAirVel;
    public Vector3 moveVelocity;

    //Used to scale Movement dynamics to avatar's size
    public float scaleBy = 1f;
    float specialScale = 1.6f;

    //Used for turning and other such drab.
    Quaternion dirQuaternion;
    Vector3 dirVector;
    Rigidbody rigidbody;
    CharacterController charCtrl;
    Collider charCollider;
    Animator anim;
    bool isRunning;

    public bool groundbound = false;

    public bool isDizzy = false;
    public int maxDizzyDuration = 400;
    public int curDizzyDuration = 0;

    public int booksNum = 0;
    public bool hasCoffee = false;
    public float caffiene = 1f;
    public float bookWt = 1f;
    Vector3 originSpeed;
    public int maxHyperDuration = 400;
    public int curHyperDuration = 400;

    public AudioSource jumpSFX1;
    public AudioSource jumpSFX2;
    public AudioSource dizzySFX;

    bool restrained = true;
    Vector3 angVelocity;


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        charCtrl = GetComponent<CharacterController>();
        charCollider = GetComponent<BoxCollider>();
        anim = GetComponent<Animator>();
        transform.localScale = new Vector3(scaleBy, scaleBy, scaleBy);

        dizzySFX.Pause();

        Physics.gravity = new Vector3(0f, -9.81f, 0f);
    }

    void Update()
    {
        if (!TimeKeeper.isPaused)
        {
            if (!restrained)
                dirVector = new Vector3(0f, 0f, 0f);

            BooksHandler();
            CoffeeHandler();
            DizzyHandler();
            AnimsHandler("Jump",dirVector, Input.GetButton("Fire3"));
            Vector3.Normalize(dirVector);
            Point(dirVector);
            Move(dirVector, Input.GetButton("Fire3"));
            jump("Jump", Input.GetButton("Fire3"));
            testScript(); //for testing only
        }
    }

    void testScript()
    {
        groundbound = IsGrounded();
    }

    void AnimsHandler(string jump,Vector3 dirVector, bool running)
    {
        if(IsGrounded()) {
            if (anim.GetBool("Jumping")) anim.SetBool("Jumping", false);
            if (anim.GetBool("DoubleJumping")) anim.SetBool("DoubleJumping", false);
            if (Input.GetButtonDown(jump)) {
                if (!anim.GetBool("Jumping")) anim.SetBool("Jumping", true);
                if (anim.GetBool("Running")) anim.SetBool("Running", false);
                if (anim.GetBool("Walking")) anim.SetBool("Walking", false);
            } else {
                if(dirVector.magnitude > 0f) {
                    if(running) {
                        if (!anim.GetBool("Running")) anim.SetBool("Running",true);
                        if (anim.GetBool("Walking")) anim.SetBool("Walking", false);
                    } else {
                        if (anim.GetBool("Running")) anim.SetBool("Running", false);
                        if (!anim.GetBool("Walking")) anim.SetBool("Walking", true);
                    }
                } else {
                    if (anim.GetBool("Running")) anim.SetBool("Running", false);
                    if (anim.GetBool("Walking")) anim.SetBool("Walking", false);
                }
            }
        } else {
            if (anim.GetBool("Running")) anim.SetBool("Running", false);
            if (anim.GetBool("Walking")) anim.SetBool("Walking", false);
            if (canDoubleJump){
                if (Input.GetButtonDown(jump))
                {
                    if (anim.GetBool("Jumping")) anim.SetBool("Jumping", false);
                    if (!anim.GetBool("DoubleJumping")) anim.SetBool("DoubleJumping", true);
                } else {
                    if (!anim.GetBool("Jumping")) anim.SetBool("Jumping", true);
                    if (anim.GetBool("DoubleJumping")) anim.SetBool("DoubleJumping", false);
                }
            }
        }
    }

    void BooksHandler()
    {
        if (booksNum > 0)
        {
            //change vel per book
            bookWt = 1f - ((float)booksNum * .2f);
        }
    }

    void CoffeeHandler()
    {
        if (hasCoffee)
        {
            //make "hyper" for seconds
            caffiene = 2f;
            curHyperDuration -= 1;
            if (curHyperDuration <= 0)
            {
                caffiene = 1f;
                curHyperDuration = maxHyperDuration;
                hasCoffee = false;
            }
        }
    }

    void DizzyHandler()
    {
        if (isDizzy)
        {
            dirVector.Set(-1f * Input.GetAxisRaw("Horizontal"), 0f, -1f * Input.GetAxisRaw("Vertical"));
            curDizzyDuration -= 1;
            if (curDizzyDuration <= 0)
            {
                curDizzyDuration = maxDizzyDuration;
                isDizzy = false;
            }

            dizzySFX.UnPause();
        }
        else
        {
            dirVector.Set(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            dizzySFX.Pause();
        }
    }

    //Moves the player
    void Move(Vector3 dirVector, bool running)
    {
        if (caffiene > 1f)
        {
            rigidbody.position += moveVelocity * caffiene;
        }
        else
        {
            rigidbody.position += moveVelocity * bookWt;
        }

        if (IsGrounded())
        {
            if (running)
            {

                xVel += dirVector.x * scaleBy * specialScale * runAccel * TimeKeeper.GetDeltaTime();
                zVel += dirVector.z * scaleBy * specialScale * runAccel * TimeKeeper.GetDeltaTime();
                moveVelocity.Set(xVel, 0f, zVel);
            }
            else
            {
                moveVelocity = dirVector * scaleBy * specialScale * walkSpeed * TimeKeeper.GetDeltaTime();
            }
            if (moveVelocity.magnitude < scaleBy * specialScale * 2f) maxAirVel = scaleBy * specialScale * 2f;
            else
            {
                maxAirVel = moveVelocity.magnitude;
            }

        }
        else
        {
            moveVelocity += dirVector * scaleBy * specialScale * airAccel * TimeKeeper.GetDeltaTime();
            moveVelocity *= airDrag;
            moveVelocity = Vector3.ClampMagnitude(moveVelocity, maxAirVel);
        }
        // regulates running friction
        if (xVel != 0f)
        {
            if (xVel < 0.025f && xVel > -0.025f) xVel = 0f;
            else
            {
                xVel *= friction;
            }
        }
        if (zVel != 0f)
        {
            if (zVel < 0.025f && zVel > -0.025f) zVel = 0f;
            else
            {
                zVel *= friction;
            }
        }
    }

    //Points the player
    void Point(Vector3 dirVector)
    {
        if (dirVector.sqrMagnitude > 0)
        {
            if (IsGrounded())
            {
                transform.LookAt(transform.position + Quaternion.Euler(0, -90, 0) * dirVector);
            }
        }
    }

    //Makes the player jump
    void jump(string input, bool running)
    {
        if (IsGrounded())
        {
            if (Input.GetButtonDown(input))
            {
                if (running) rigidbody.velocity = new Vector3(rigidbody.velocity.x, scaleBy * specialScale * runJumpVel, rigidbody.velocity.z);
                else rigidbody.velocity = new Vector3(rigidbody.velocity.x, scaleBy * specialScale * walkJumpVel, rigidbody.velocity.z);
                jumpSFX1.Play();
            }
            else rigidbody.velocity.Set(rigidbody.velocity.x, 0f, rigidbody.velocity.y);
            canDoubleJump = true;
        }
        else
        {
            if (canDoubleJump && Input.GetButtonDown(input))
            {
                if (running) rigidbody.velocity = new Vector3(rigidbody.velocity.x, 1.25f * scaleBy * specialScale * runJumpVel, rigidbody.velocity.z);
                else rigidbody.velocity = new Vector3(rigidbody.velocity.x, 1.25f * scaleBy * specialScale * walkJumpVel, rigidbody.velocity.z);
                canDoubleJump = false;
                jumpSFX2.Play();
            }
            if (rigidbody.velocity.y < 0)
            {
                rigidbody.velocity += Vector3.up * scaleBy * specialScale * Physics.gravity.y * (fallMultiplier - 1) * TimeKeeper.GetDeltaTime();
            }
            else if (rigidbody.velocity.y > 0 && !Input.GetButton(input))
            {
                rigidbody.velocity += Vector3.up * scaleBy * specialScale * Physics.gravity.y * (lowJumpMultiplier - 1) * TimeKeeper.GetDeltaTime();
            }
            rigidbody.velocity += Vector3.up * scaleBy * specialScale * Physics.gravity.y * bonusGravityMult * TimeKeeper.GetDeltaTime();
        }
    }


    bool IsGrounded()
    {
        RaycastHitRenderer hitInfo;
        Ray downRay = new Ray(transform.position, -Vector3.up);
        return SuperRaycast.Raycast(downRay, out hitInfo, distToGround + 0.1f);
    }


    void OnTriggerEnter(Collider c)
    {
        if (c.tag == "Default")
        {
            isDizzy = true;
        }
    }


    public void GoNuts()
    {
        restrained = false;

        rigidbody.constraints = RigidbodyConstraints.None;

        rigidbody.velocity = new Vector3(scaleBy * specialScale * runJumpVel, 2f * scaleBy * specialScale * runJumpVel, 0f);
        rigidbody.angularVelocity = new Vector3(0f, 0f, -135f);

        Physics.gravity = new Vector3(4f, -4f, 0f);
    }
}

