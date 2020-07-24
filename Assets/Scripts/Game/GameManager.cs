using Colyseus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AvatarMoveEvent : UnityEvent<Vector3, Quaternion> { };
public class AvatarEmoteEvent : UnityEvent<int> { };
public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject ballPrefab;

    public AvatarMoveEvent onAvatarMove = new AvatarMoveEvent();
    public AvatarEmoteEvent onAvatarEmote = new AvatarEmoteEvent();
    [HideInInspector]
    public UnityEvent onAvatarInteract = new UnityEvent();

    private GameObject entitiesContainer;
    private GameObject propsContainer;
    private Avatar avatar;
    private readonly Dictionary<string, Player> players = new Dictionary<string, Player>();
    private LightSwitch lightSwitch;
    private readonly List<Door> door = new List<Door>();
    private BallSpawner ballSpawner;
    private BallDestroyer ballDestroyer;
    private readonly Dictionary<string, Ball> balls = new Dictionary<string, Ball>();
    private readonly float sendInterval = 50 / 1000f;
    private float sendTimer = 0f;


    // Start is called before the first frame update
    void Start()
    {
        entitiesContainer = transform.Find("Entities").gameObject;
        propsContainer = transform.Find("Props").gameObject;
        avatar = entitiesContainer.transform.Find("Avatar").GetComponent<Avatar>();
        lightSwitch = propsContainer.transform.Find("LightSwitch").GetComponent<LightSwitch>();
        door.Add(propsContainer.transform.Find("Doors").Find("DoorLeft").GetComponent<Door>());
        door.Add(propsContainer.transform.Find("Doors").Find("DoorRight").GetComponent<Door>());
        ballSpawner = propsContainer.transform.Find("BallSpawner").GetComponent<BallSpawner>();
        ballDestroyer = propsContainer.transform.Find("BallDestroyer").GetComponent<BallDestroyer>();
        LeanTween.init(800);
        avatar.transform.position = Vector3.zero;
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
        if (door[0].GetState() != value)
        {
            for (int i = 0; i < door.Count; i++)
            {
                door[i].SetState(value);
            }
        }
    }
    public void SpawnBall(string id, BallData bd)
    {
        Ball newBall = Instantiate(ballPrefab, propsContainer.transform).GetComponent<Ball>();
        newBall.transform.position = new Vector3(bd.position.x,bd.position.y,bd.position.z);
        newBall.transform.rotation = new Quaternion(bd.rotation.x, bd.rotation.y, bd.rotation.z, bd.rotation.w);
        newBall.SetPositionTarget(new Vector3(bd.position.x, bd.position.y, bd.position.z));
        newBall.SetRotationTarget(new Quaternion(bd.rotation.x, bd.rotation.y, bd.rotation.z, bd.rotation.w));
        balls.Add(id,newBall);
        ballSpawner.PlaySpawnSound();
    }
    public void RemoveBall(string id)
    {
        Ball removedBall = balls[id];
        Destroy(removedBall.gameObject);
        players.Remove(id);
        ballDestroyer.BallDestroyed();
    }
    public void BallOwnerChanged(string id, string playerId)
    {
        Ball ball = balls[id];
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
            if (ball.transform.parent == avatar.transform)
            {
                avatar.PlayThrowBallSound();
            } else
            {
                ball.transform.parent.GetComponent<Player>().PlayThrowBallSound();
            }
            ball.SetOwner(null);
        }
    }
    public void BallMoved(string id, Vector3 position)
    {
        Ball ball = balls[id];
        ball.SetPositionTarget(position);
    }
    public void BallRotated(string id, Quaternion rotation)
    {
        Ball ball = balls[id];
        ball.SetRotationTarget(rotation);
    }
}
