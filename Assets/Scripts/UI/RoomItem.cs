using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class RoomItemEvent : UnityEvent { };
public class RoomItem : MonoBehaviour
{
    private TextMeshProUGUI roomNameUI;
    private Button joinButton;
    public string roomId;
    public RoomItemEvent onJoinRoom = new RoomItemEvent();
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("start");
        roomNameUI = transform.Find("Name").GetComponent<TextMeshProUGUI>();
        joinButton = transform.Find("JoinButton").GetComponent<Button>();
        joinButton.onClick.AddListener(()=> onJoinRoom.Invoke());
    }

    public void SetName(string name)
    {
        Debug.Log("set name");
        roomNameUI.text = name;
    }
    public void SetId(string id)
    {
        roomId = id;
    }
}
