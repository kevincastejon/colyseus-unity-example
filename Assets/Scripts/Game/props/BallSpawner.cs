using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    private SoundsPlayer soundsPlayer;
    // Start is called before the first frame update
    void Start()
    {
        soundsPlayer = transform.Find("Sounds").GetComponent<SoundsPlayer>();
    }
    
    public void PlaySpawnSound()
    {
        soundsPlayer.Play(0);
    }
}
