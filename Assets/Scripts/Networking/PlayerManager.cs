using KinematicCharacterController;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public event System.Action<bool> OnPlayerReadyStatusChanged;

    public static readonly HashSet<string> playerNames = new HashSet<string>();

    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    public string PlayerName;

    [Header("References")]
    [SerializeField] private NetworkConnection connection;
    [SerializeField] private LobbyPlayerUI lobbyUIObjectPrefab;
    [SerializeField] private LobbyPlayerUI lobbyUIObject;
    [SerializeField] public BattlePlayer battlePlayer;
    public NetworkIdentity Identity { get; private set; }

    /// <summary>
    /// Diagnostic flag indicating whether this player is ready for the game to begin.
    /// <para>Invoke CmdChangeReadyState method on the client to set this flag.</para>
    /// <para>When all players are ready to begin, the game will start. This should not be set directly, CmdChangeReadyState should be called on the client to set it on the server.</para>
    /// </summary>
    [Tooltip("Diagnostic flag indicating whether this player is ready for the game to begin")]
    [SyncVar(hook = nameof(PlayerReadyStateChanged))]
    public bool ReadyToBegin = false;

    // RuntimeInitializeOnLoadMethod -> fast playmode without domain reload
    [UnityEngine.RuntimeInitializeOnLoadMethod]
    static void ResetStatics()
    {
        playerNames.Clear();
    }

    private void Start()
    {
        Identity = GetComponent<NetworkIdentity>();
    }

    private void OnSpawned()
    {

    }

    private void AddSelfToLobbyList(BattlegolfNetworkManager lobby)
    {
        bool alreadyInList = false;
        foreach (var player in lobby.GetPlayers())
        {
            if (player == this)
                alreadyInList = true;
        }

        if (!alreadyInList)
            lobby.AddPlayerManagerToPlayerList(this);
    }


    void OnPlayerNameChanged(string _, string newName)
    {
        LobbyUI.instance.localPlayerName = PlayerName;
    }

    #region Commands

    [Command]
    void CmdChangeReadyStatus(bool newValue)
    {
        ReadyToBegin = newValue;
    }

    #endregion

    void PlayerReadyStateChanged(bool oldValue, bool newValue)
    {
        CmdChangeReadyStatus(newValue);
        OnPlayerReadyStatusChanged?.Invoke(newValue);
        if (NetworkManager.singleton is BattlegolfNetworkManager lobby)
        {
            lobby.ReadyStatusChanged();
        }
    }


    public override void OnStartServer()
    {
        base.OnStartServer();
        PlayerName = (string)connectionToClient.authenticationData;
    }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient()
    {
        if (LobbyUI.instance != null)
        {
            LobbyUI.instance.readyToggle.onValueChanged.AddListener(CmdChangeReadyStatus);
            lobbyUIObject = Instantiate(lobbyUIObjectPrefab, LobbyUI.instance.userListGrid.transform);
            lobbyUIObject.usernameText.text = PlayerName;
            lobbyUIObject.readyObject.SetActive(false);

            OnPlayerReadyStatusChanged += lobbyUIObject.OnReadyStateChanged;

            OnPlayerReadyStatusChanged.Invoke(false);
        }

        //TODO: this is a fucking mess
        if (NetworkManager.singleton is BattlegolfNetworkManager lobby)
        {
            AddSelfToLobbyList(lobby);

            //Spawn the battleplayer
            if (lobby.CurrentState == BattlegolfNetworkState.Battle)
            {
                GameObject bp = Instantiate(lobby.GetBattleplayerPrefab());
                bp.GetComponent<BattlePlayer>().SetManager(this);
                
                NetworkServer.Spawn(bp, this.gameObject);
                bp.GetComponent<BattlePlayer>().OnNetworkSpawn();

                this.battlePlayer = bp.GetComponent<BattlePlayer>();

                lobby.NumSpawnedPlayers++;

                //Spawn the battleplayer's pawns
                for (int i = 0; i < GameManager.instance.NumStartingPawns; i++)
                {
                    GameObject go = Instantiate(lobby.GetPawnPrefab(),
                        GameManager.instance.WorldGen.PawnSpawns[i + ((lobby.NumSpawnedPlayers - 1) * GameManager.instance.NumStartingPawns)],
                        new Quaternion(0, 0, 0, 0));
                    go.GetComponent<Pawn>().SetOwner(bp.GetComponent<BattlePlayer>());
                    NetworkServer.Spawn(go, this.gameObject);
                }

                GameManager.instance.AddPlayer(bp.GetComponent<BattlePlayer>());

                //TODO: this shouldnt be in a player start function
                if (lobby.NumSpawnedPlayers == NetworkServer.connections.Count)
                {
                    GameManager.instance.OnAllPlayersSpawned();
                }
            }
            else Debug.Log("LobbyPlayer could not find a BattlegolfNetworkManager");
        }
    }

    public override void OnStopClient()
    {
        if(LobbyUI.instance != null)
        {
            LobbyUI.instance.readyToggle.onValueChanged.RemoveListener(CmdChangeReadyStatus);
            OnPlayerReadyStatusChanged -= lobbyUIObject.OnReadyStateChanged;
        }

        Destroy(lobbyUIObject);
    }

    public override void OnStartLocalPlayer()
    {
        
    }
}
