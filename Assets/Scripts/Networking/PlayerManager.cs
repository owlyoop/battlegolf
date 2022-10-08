using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public event System.Action<bool> OnPlayerReadyStatusChanged;

    public static readonly HashSet<string> playerNames = new HashSet<string>();

    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    public string playerName;

    public BattlePlayer battlePlayer;
    public NetworkIdentity identity;

    public LobbyPlayerUI lobbyUIObjectPrefab;
    public LobbyPlayerUI lobbyUIObject;

    /// <summary>
    /// Diagnostic flag indicating whether this player is ready for the game to begin.
    /// <para>Invoke CmdChangeReadyState method on the client to set this flag.</para>
    /// <para>When all players are ready to begin, the game will start. This should not be set directly, CmdChangeReadyState should be called on the client to set it on the server.</para>
    /// </summary>
    [Tooltip("Diagnostic flag indicating whether this player is ready for the game to begin")]
    [SyncVar(hook = nameof(PlayerReadyStateChanged))]
    public bool readyToBegin = false;

    // RuntimeInitializeOnLoadMethod -> fast playmode without domain reload
    [UnityEngine.RuntimeInitializeOnLoadMethod]
    static void ResetStatics()
    {
        playerNames.Clear();
    }

    private void Start()
    {
        if (NetworkManager.singleton is BattlegolfNetworkManager lobby)
        {
            bool alreadyInList = false;
            foreach (var player in lobby.players)
            {
                if (player == this)
                    alreadyInList = true;
            }

            if (!alreadyInList)
                lobby.players.Add(this);
        }

        else Debug.LogError("LobbyPlayer could not find a BattlegolfNetworkManager");
    }

    void OnPlayerNameChanged(string _, string newName)
    {
        LobbyUI.instance.localPlayerName = playerName;
    }

    #region Commands

    [Command]
    void CmdChangeReadyStatus(bool newValue)
    {
        readyToBegin = newValue;
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
        playerName = (string)connectionToClient.authenticationData;
        //TODO: probably move this into GameManager.cs
        if (NetworkManager.singleton is BattlegolfNetworkManager lobby)
        {
            if (lobby.currentState == BattlegolfNetworkState.Battle)
            {
                identity = GetComponent<NetworkIdentity>();
                GameObject bp = Instantiate(lobby.spawnPrefabs[0]);
                battlePlayer = bp.GetComponent<BattlePlayer>();
                battlePlayer.manager = this;
                battlePlayer.GetComponent<PlayerInput>().worldOverviewControl = battlePlayer.overviewController.GetComponent<PawnController>();
                //battlePlayer.GetComponent<PlayerInput>().Initialize();
                NetworkServer.Spawn(bp, this.gameObject);


                for (int i = 0; i < GameManager.instance.numStartingPawns; i++)
                {
                    GameObject go = Instantiate(lobby.spawnPrefabs[1], GameManager.instance.worldGen.pawnSpawns[i], new Quaternion(0,0,0,0));
                    Pawn pawn = go.GetComponent<Pawn>();
                    battlePlayer.ownedPawns.Add(pawn);
                    pawn.owner = battlePlayer;
                    NetworkServer.Spawn(go, battlePlayer.gameObject);
                }

                lobby.players.RemoveAll(PlayerManager => PlayerManager == null);
            }
        }
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
            lobbyUIObject.usernameText.text = playerName;
            lobbyUIObject.readyObject.SetActive(false);

            OnPlayerReadyStatusChanged += lobbyUIObject.OnReadyStateChanged;

            OnPlayerReadyStatusChanged.Invoke(false);
        }

        if (NetworkManager.singleton is BattlegolfNetworkManager lobby)
        {
            if (lobby.currentState == BattlegolfNetworkState.Battle)
            {
            }
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
        base.OnStartLocalPlayer();
    }
}
