using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    //private PlayerData state;
    private GameObject camera;
    private TextMeshProUGUI playerName;
    private CharacterSkinController skinController;
    private Rigidbody rigidBody;
    private Animator animator;
    private Vector3 posTarget;
    private Quaternion rotTarget;
    private int emote;
    private SoundsPlayer soundsPlayer;

    void Awake()
    {
        camera = GameObject.Find("Main Camera").gameObject;
        playerName = transform.Find("Canvas").Find("Name").GetComponent<TextMeshProUGUI>();
        skinController = GetComponent<CharacterSkinController>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        soundsPlayer = transform.Find("Sounds").GetComponent<SoundsPlayer>();
    }
    private void Update()
    {
        playerName.transform.rotation = Quaternion.LookRotation(playerName.transform.position - camera.transform.position);
    }
    void FixedUpdate()
    {
        if (posTarget != null && Vector3.Distance(transform.position, posTarget) > 0.1f)
        {
            transform.rotation = rotTarget;
            Vector3 dir = (posTarget - transform.position).normalized;
            rigidBody.velocity = dir * Time.fixedDeltaTime * 500;
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
    public void SetPositionTarget(Vector3 posTarget)
    {
        this.posTarget = posTarget;
    }
    public void SetRotationTarget(Quaternion rotTarget)
    {
        this.rotTarget = rotTarget;
    }

    public void SetName(string name)
    {
        playerName.SetText(name);
    }
    public string GetName()
    {
        return playerName.text;
    }
    public void SetColor(int color)
    {
        skinController.ChangeMaterialSettings(color == 0 ? 0 : 2);
        playerName.color = color == 0 ? Color.red : Color.blue;
    }
    public int GetColor()
    {
        return (skinController.colorIndex);
    }
    public void SetEmote(int index)
    {
        emote = index;
        skinController.ChangeEmote(index);
    }
    public int GetEmote()
    {
        return emote;
    }
    public void PlayThrowBallSound()
    {
        soundsPlayer.Play(1);
    }
}