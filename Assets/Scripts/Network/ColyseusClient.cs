using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using GameDevWare.Serialization;

[Serializable]
class Metadata
{
    public string name;
}
//class Vec3
//{
//    public float x;
//    public float y;
//    public float z;
//}
class PlayerInputMessage
{
    public Vect3 position;
    public Quat rotation;
}
[Serializable]
class RoomListingData
{
    public double clients;
    public bool locked;
    public bool isPrivate;
    public double maxClients;
    public Metadata metadata;
    public string name;
    public string processId;
    public string roomId;
    public bool unlisted;
}
public class ColyseusClient : MonoBehaviour
{
    public string roomType = "chatroom";
    private Menu menu;
    private GameManager gameManager;
    private string username;
    private int color;
    protected Client client;
    protected Room<State> room;
    protected Room<IndexedDictionary<string, object>> lobbyRoom;

    // Use this for initialization
    void Start()
    {
        gameManager = GameObject.Find("Map").GetComponent<GameManager>();
        //gameManager.gameObject.SetActive(false);
        menu = GameObject.Find("Menu").GetComponent<Menu>();
        menu.onJoinRoom.AddListener((string roomId) => { JoinRoom(roomId); });
        menu.onChangeUsername.AddListener((string newUsername) => { SetUserName(newUsername); });
        menu.onChangeColor.AddListener((string newColor) => { SetColor(newColor); });
        menu.onCreateRoom.AddListener((string roomName) => { CreateRoom(roomName); });
        menu.Init();
        Connect();
    }

    void Connect()
    {
        string endpoint = "ws://localhost:2567";
        Debug.Log("Connecting to " + endpoint);
        client = ColyseusManager.Instance.CreateClient(endpoint);
        JoinLobbyRoom();
    }

    public void SetUserName(string username)
    {
        //Debug.Log("SET USERNAME "+username);
        this.username = username;
        PlayerPrefs.SetString("username", username);
    }
    public void SetColor(string color)
    {
        //Debug.Log("SET TEAM "+team);
        this.color = color == "red" ? 0 : 1;
        PlayerPrefs.SetInt("color", this.color);
    }

    public async void JoinLobbyRoom()
    {
        lobbyRoom = await client.JoinOrCreate<IndexedDictionary<string, object>>("lobbyroom");
        lobbyRoom.OnMessage("rooms", (RoomListingData[] rooms) =>
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                menu.AddRoom(rooms[i].roomId, rooms[i].metadata.name);
            }
        });
        lobbyRoom.OnMessage("+", (object[] room) =>
        {
            string roomId = (string)room[0];
            RoomListingData roomData = ObjectExtensions.ToObject<RoomListingData>(room[1]);
            menu.AddRoom(roomId, roomData.metadata.name);
        });
        lobbyRoom.OnMessage("-", (string roomId) =>
        {
            menu.RemoveRoom(roomId);
        });
    }

    public async void CreateRoom(string roomName)
    {
        menu.gameObject.SetActive(false);
        if (lobbyRoom != null)
        {
            await lobbyRoom.Leave();
        }
        room = await client.Create<State>(roomType, new Dictionary<string, object>() {
            { "name", roomName },
            { "username", username },
            { "color", color },
        });
        RegisterRoomHandlers();
        StartGame();
    }
    public async void JoinRoom(string roomId)
    {
        if (lobbyRoom != null)
        {
            await lobbyRoom.Leave();
        }
        menu.gameObject.SetActive(false);
        room = await client.JoinById<State>(roomId, new Dictionary<string, object>() {
            { "username", username },
            { "color", color },
        });
        RegisterRoomHandlers();
        StartGame();
    }
    public void RegisterRoomHandlers()
    {
        room.State.players.OnAdd += OnPlayerAdd;
        room.State.players.OnRemove += OnPlayerRemove;
        room.State.lights.OnChange += OnLightsChange;
        room.State.doors.OnChange += OnDoorsChange;
        room.State.ball.OnChange += OnBallChange;

        room.OnLeave += (code) => Debug.Log("ROOM: ON LEAVE");
        room.OnError += (code, message) => Debug.LogError("ERROR, code =>" + code + ", message => " + message);
    }

    void StartGame()
    {
        gameManager.gameObject.SetActive(true);
        gameManager.SetAvatarColor(color == 0 ? 0 : 2);
        gameManager.onAvatarMove.AddListener((Vector3 pos, Quaternion rot) => AvatarMoved(pos, rot));
        gameManager.onAvatarEmote.AddListener((int emote) => AvatarEmoted(emote));
        gameManager.onAvatarInteract.AddListener(() => AvatarInteracted());

    }

    void AvatarMoved(Vector3 _pos, Quaternion _rot)
    {
        PlayerInputMessage msg = new PlayerInputMessage();
        Vect3 pos = new Vect3();
        pos.x = _pos.x;
        pos.y = _pos.y;
        pos.z = _pos.z;
        Quat rot = new Quat();
        rot.x = _rot.x;
        rot.y = _rot.y;
        rot.z = _rot.z;
        rot.w = _rot.w;
        msg.position = pos;
        msg.rotation = rot;
        room.Send("input", msg);
    }
    void AvatarEmoted(int emote)
    {
        room.Send("emote", emote);
    }
    void AvatarInteracted()
    {
        room.Send("interact");
    }


    void OnPlayerAdd(PlayerData pd, string key)
    {
        if (key != room.SessionId)
        {
            Debug.Log("Player joined");
            gameManager.AddPlayer(key, pd);
            pd.OnChange += (List<Colyseus.Schema.DataChange> changes) => OnPlayerChange(changes, key);
        }
    }

    void OnPlayerRemove(PlayerData pd, string key)
    {
        if (key != room.SessionId)
        {
            Debug.Log("Player left");
            gameManager.RemovePlayer(key, pd);
        }
    }

    void OnPlayerChange(List<Colyseus.Schema.DataChange> changes, string key)
    {
        for (int i = 0; i < changes.Count; i++)
        {
            if (changes[i].Field == "emote")
            {
                gameManager.PlayerChangedEmote(key, (int)((uint)changes[i].Value));
            }
            else if (changes[i].Field == "position")
            {
                Vect3 pos = (Vect3)changes[i].Value;
                gameManager.PlayerMoved(key, new Vector3(pos.x, pos.y, pos.z));
            }
            else if (changes[i].Field == "rotation")
            {
                Quat rot = (Quat)changes[i].Value;
                gameManager.PlayerRotated(key, new Quaternion(rot.x, rot.y, rot.z, rot.w));
            }
        }
    }

    void OnLightsChange(List<Colyseus.Schema.DataChange> changes)
    {
        Debug.Log("Lights changed " + (bool)(changes[0].Value));
        gameManager.LightsChanged((bool)(changes[0].Value));
    }
    void OnDoorsChange(List<Colyseus.Schema.DataChange> changes)
    {
        Debug.Log("Doors changed " + (bool)(changes[0].Value));
        gameManager.DoorsChanged((bool)(changes[0].Value));
    }
    void OnBallChange(List<Colyseus.Schema.DataChange> changes)
    {
        for (int i = 0; i < changes.Count; i++)
        {
            if (changes[i].Field == "owner")
            {
                Debug.Log("Ball owner changed");
                gameManager.BallOwnerChanged((string)changes[i].Value);
            }
            else if (changes[i].Field == "position")
            {
                Vect3 pos = (Vect3)changes[i].Value;
                //Debug.Log("Ball changed " + pos.x + " " + pos.y + " " + pos.z);
                gameManager.BallMoved(new Vector3(pos.x, pos.y, pos.z));
            }
            else if (changes[i].Field == "rotation")
            {
                Quat rot = (Quat)changes[i].Value;
                //Debug.Log("Ball changed " + rot.x + " " + rot.y + " " + rot.z + " " + rot.w);
                gameManager.BallRotated(new Quaternion(rot.x, rot.y, rot.z, rot.w));
            }
        }
    }

    void OnApplicationQuit()
    {

    }
}
