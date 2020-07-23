using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    LTDescr tween;
    bool opened;
    GameObject door;
    private SoundsPlayer soundsPlayer;
    private void Start()
    {
        door = transform.Find("Inner").gameObject;
        soundsPlayer = GetComponent<SoundsPlayer>();
    }
    public void SetState(bool open)
    {
        Debug.Log("DOOR " + open);
        if (tween != null)
        {
            LeanTween.cancel(tween.id);
        }
        opened = open;
        tween = LeanTween.moveLocal(door, new Vector3(open ? 2.8f : 0f, door.transform.localPosition.y, door.transform.localPosition.z), 0.5f);
        tween.setOnComplete(() => tween = null);
        soundsPlayer.Play(open ? 1 : 0);
    }
    public bool GetState()
    {
        return opened;
    }
}
