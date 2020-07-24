using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDestroyer : MonoBehaviour
{
    private SoundsPlayer soundsPlayer;
    private GameObject light;
    // Start is called before the first frame update
    void Start()
    {
        soundsPlayer = transform.Find("Sounds").GetComponent<SoundsPlayer>();
        light = transform.Find("Light").gameObject;
    }

    private void Update()
    {
        if (light.activeSelf && !soundsPlayer.IsPlaying(0))
        {
            light.SetActive(false);
        }
    }

    public void BallDestroyed()
    {
        soundsPlayer.Play(0);
        light.SetActive(true);
    }
}
