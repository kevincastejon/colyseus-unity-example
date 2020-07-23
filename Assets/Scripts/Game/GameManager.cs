using Colyseus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AvatarMoveEvent : UnityEvent<Vector3, Quaternion> { };
public class AvatarEmoteEvent : UnityEvent<int> { };
public class GameManager : MonoBehaviour
{
    public GameObject computerPrefab;
    public GameObject playerPrefab;
    public AvatarMoveEvent onAvatarMove = new AvatarMoveEvent();
    public AvatarEmoteEvent onAvatarEmote = new AvatarEmoteEvent();
    [HideInInspector]
    public UnityEvent onAvatarInteract = new UnityEvent();

    private GameObject entitiesContainer;
    private Avatar avatar;
    private readonly Dictionary<string, Player> players = new Dictionary<string, Player>();
    private LightSwitch lightSwitch;
    private readonly List<Door> doors = new List<Door>();
    private Ball ball;
    private readonly float sendInterval = 50 / 1000f;
    private float sendTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        entitiesContainer = transform.Find("Entities").gameObject;
        avatar = entitiesContainer.transform.Find("Avatar").GetComponent<Avatar>();
        lightSwitch = transform.Find("Props").Find("LightSwitch").GetComponent<LightSwitch>();
        doors.Add(transform.Find("Props").Find("Doors").Find("DoorLeft").GetComponent<Door>());
        doors.Add(transform.Find("Props").Find("Doors").Find("DoorRight").GetComponent<Door>());
        ball = transform.Find("Props").Find("Ball").GetComponent<Ball>();

        Quaternion q = new Quaternion(0,1,0,0);
        Vector3 v = new Vector3(0,0,1);
        //Debug.Log(q*v);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            avatar.ChangeEmote(0);
            onAvatarEmote.Invoke(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            avatar.ChangeEmote(1);
            onAvatarEmote.Invoke(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            avatar.ChangeEmote(2);
            onAvatarEmote.Invoke(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            avatar.ChangeEmote(3);
            onAvatarEmote.Invoke(3);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            onAvatarInteract.Invoke();
        }
        sendTimer += Time.deltaTime;

        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            onAvatarMove.Invoke(avatar.transform.position, avatar.transform.rotation);
        }
    }
    public void SetAvatarColor(int color)
    {
        avatar.ChangeColor(color);
    }

    public void AddPlayer(string id, PlayerData pd)
    {
        Player newPlayer = Instantiate(playerPrefab, entitiesContainer.transform).GetComponent<Player>();
        newPlayer.transform.position = new Vector3(pd.position.x, pd.position.y, pd.position.z);
        newPlayer.transform.rotation = new Quaternion(pd.rotation.x, pd.rotation.y, pd.rotation.z, pd.rotation.w);
        newPlayer.SetPositionTarget(new Vector3(pd.position.x, pd.position.y, pd.position.z));
        newPlayer.SetRotationTarget(new Quaternion(pd.rotation.x, pd.rotation.y, pd.rotation.z, pd.rotation.w));
        newPlayer.SetName(pd.name);
        newPlayer.SetColor((int)pd.color);
        players.Add(id, newPlayer);
    }

    public void RemovePlayer(string id, PlayerData pd)
    {
        Player leftPlayer = players[id];
        Destroy(leftPlayer.gameObject);
        players.Remove(id);
    }

    public void PlayerMoved(string id, Vector3 position)
    {
        Player player = players[id];
        Vector3 targetPos = new Vector3(position.x, position.y, position.z);
        if (Vector3.Distance(targetPos, player.transform.position) > 0.01f)
        {
            player.SetPositionTarget(targetPos);
        }
    }
    public void PlayerRotated(string id, Quaternion rotation)
    {
        Player player = players[id];
        if (Vector3.Distance(rotation.eulerAngles, player.transform.eulerAngles) > 0.01f)
        {
            player.SetRotationTarget(rotation);
        }
    }
    public void PlayerChangedEmote(string id, int emote)
    {
        Player player = players[id];
        if (emote != player.GetEmote())
        {
            player.SetEmote(emote);
        }
    }

    public void LightsChanged(bool value)
    {
        if (lightSwitch.GetLights() != value)
        {
            lightSwitch.SetLights(value);
            lightSwitch.transform.eulerAngles = new Vector3(value ? 270 : 90, 0, 90);
        }
    }
    public void DoorsChanged(bool value)
    {
        if (doors[0].GetState() != value)
        {
            for (int i = 0; i < doors.Count; i++)
            {
                doors[i].SetState(value);
            }
        }
    }
    public void BallOwnerChanged(string playerId)
    {
        if (playerId != "")
        {
            if (players.ContainsKey(playerId))
            {
                ball.SetOwner(players[playerId].gameObject);
            }
            else
            {
                ball.SetOwner(avatar.gameObject);
            }
        }
        else
        {
            ball.SetOwner(null);
        }
    }
    public void BallMoved(Vector3 position)
    {
        ball.SetPositionTarget(position);
    }
    public void BallRotated(Quaternion rotation)
    {
        ball.SetRotationTarget(rotation);
    }
}
