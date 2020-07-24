using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avatar : MonoBehaviour
{
    private CharacterSkinController skinController;
    private Rigidbody rigidBody;
    private Animator animator;
    private SoundsPlayer soundsPlayer;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        skinController = GetComponent<CharacterSkinController>();
        animator = GetComponent<Animator>();
        soundsPlayer = transform.Find("Sounds").GetComponent<SoundsPlayer>();
    }

    void FixedUpdate()
    {
        //Debug.Log(transform.rotation);
        float inputX = Input.GetAxisRaw("Horizontal") > 0 ? 1 : (Input.GetAxisRaw("Horizontal") < 0 ? -1 : 0);
        float inputZ = Input.GetAxisRaw("Vertical") > 0 ? 1 : (Input.GetAxisRaw("Vertical") < 0 ? -1 : 0);
        if (inputX != 0 || inputZ != 0)
        {
            var cam = Camera.main;
            var forward = cam.transform.forward;
            var right = cam.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 desiredMoveDirection = forward * inputZ + right * inputX;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), 0.1f);

            rigidBody.velocity = desiredMoveDirection * Time.fixedDeltaTime * 500;
            animator.SetFloat("Blend", 1, 0.3f, Time.fixedDeltaTime);
            if (!soundsPlayer.IsPlaying(0))
            {
            soundsPlayer.Play(0);
            }
        }
        else
        {
            rigidBody.velocity = Vector3.zero;
            animator.SetFloat("Blend", 0, 0.15f, Time.fixedDeltaTime);
            soundsPlayer.Stop(0);
        }
    }
    public void ChangeEmote(int index)
    {
        skinController.ChangeEmote(index);
    }
    public void ChangeColor(int index)
    {
        skinController.ChangeMaterialSettings(index);
    }
    public void PlayThrowBallSound()
    {
        soundsPlayer.Play(1);
    }
}
