
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class RoomEvent : UnityEvent<string> { };
public class Menu : MonoBehaviour
{
    public GameObject roomItemPrefab;
    public RoomEvent onJoinRoom = new RoomEvent();
    public RoomEvent onCreateRoom = new RoomEvent();
    public RoomEvent onChangeUsername = new RoomEvent();
    public RoomEvent onChangeColor = new RoomEvent();
    private List<RoomItem> rooms = new List<RoomItem>();
    private GameObject roomsContainer;
    private Button createButton;
    private Button redButton;
    private Button blueButton;
    private TMP_InputField userNameInput;
    private string[] usernames = { "Coco", "Maya", "Koda", "Dexter", "Benji" };
    public void Init()
    {
        roomsContainer = transform.Find("RoomList").Find("Viewport").Find("Content").gameObject;
        userNameInput = transform.Find("Username").GetComponent<TMP_InputField>();
        createButton = transform.Find("CreateButton").GetComponent<Button>();
        redButton = transform.Find("ButtonRed").GetComponent<Button>();
        blueButton = transform.Find("ButtonBlue").GetComponent<Button>();

        string userName = PlayerPrefs.GetString("username");
        int color = PlayerPrefs.GetInt("color");
        if (userName.Length > 0)
        {
            userNameInput.text = userName;
        }
        else
        {
            userNameInput.text = usernames[Random.Range(0, usernames.Length)];
        }
        redButton.onClick.AddListener(() => onChangeColor.Invoke("red"));
        blueButton.onClick.AddListener(() => onChangeColor.Invoke("blue"));
        if (color == 0)
        {
            redButton.Select();
            redButton.onClick.Invoke();
        } else
        {
            blueButton.Select();
            blueButton.onClick.Invoke();
        }
        userNameInput.onEndEdit.AddListener((string text) => { onChangeUsername.Invoke(userNameInput.text); });
        userNameInput.onEndEdit.Invoke(userNameInput.text);
        createButton.onClick.AddListener(() => onCreateRoom.Invoke(userNameInput.text+ " game"));
    }

    public void AddRoom(string roomId, string roomName)
    {
        if (rooms.Find(x => x.roomId == roomId) != null)
        {
            return;
        }
        RoomItem roomItem = Instantiate(roomItemPrefab, roomsContainer.transform).GetComponent<RoomItem>();
        roomItem.SetId(roomId);
        roomItem.SetName(roomName);
        roomItem.onJoinRoom.AddListener(() => { onJoinRoom.Invoke(roomId); });
        rooms.Add(roomItem);
    }
    public void RemoveRoom(string roomId)
    {
        RoomItem roomItem = rooms.Find(r => r.roomId == roomId);
        if (roomItem)
        {
            rooms.Remove(roomItem);
            Destroy(roomItem.gameObject);
        }
    }
    public void ClearRooms()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            RoomItem roomItem = rooms[i];
            rooms.Remove(roomItem);
            Destroy(roomItem.gameObject);
        }
    }
}
