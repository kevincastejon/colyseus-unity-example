using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsPlayer : MonoBehaviour
{
    private AudioSource[] sounds;
    // Start is called before the first frame update
    void Start()
    {
        sounds = GetComponents<AudioSource>();
    }

    public void Play(int index)
    {
        sounds[index].Play();
    }
    public void Stop(int index)
    {
        sounds[index].Stop();
    }
    public bool IsPlaying(int index)
    {
        return sounds[index].isPlaying;
    }
}
