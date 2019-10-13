using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float turnSpeed = 20.0f;

    Animator animator;
    Rigidbody playerRigidbody;
    Vector3 movement;
    Quaternion rotation = Quaternion.identity;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        float horizontal;
        float vertical;
        bool hasHorizontalInput;
        bool hasVerticalInnput;
        bool isWalking;
        Vector3 desiredForward;

        // Get input
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // Determine direction
        movement.Set(horizontal, 0.0f, vertical);
        movement.Normalize();

        // Determine whether the player is walking and animate accordingly
        hasHorizontalInput = !Mathf.Approximately(horizontal, 0.0f);
        hasVerticalInnput = !Mathf.Approximately(vertical, 0.0f);
        isWalking = hasHorizontalInput || hasVerticalInnput;
        animator.SetBool("IsWalking", isWalking);

        if (isWalking) {
            if (!audioSource.isPlaying) {
                audioSource.Play();
            }

        } else {
            audioSource.Stop();
        }

        // Determine rotation to movement direction
        desiredForward = Vector3.RotateTowards(transform.forward, movement, turnSpeed * Time.deltaTime, 0.0f);
        rotation = Quaternion.LookRotation(desiredForward);
    }

    void OnAnimatorMove() {
        playerRigidbody.MovePosition(playerRigidbody.position + movement * animator.deltaPosition.magnitude);
        playerRigidbody.MoveRotation(rotation);
    }
}
