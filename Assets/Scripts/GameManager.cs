using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public static GameManager GetInstance()
    {
        return instance;
    }

    public enum GameState
    {
        WaitingForWorldGeneration,
        PlayerSelectingPawn,
        PawnMoving,
        PawnFiring,
        FollowProjectile
    }

    [Header("Ingame Match Config")]
    public int numPlayers = 1;
    public int numStartingPawns = 4;
    public float turnLengthInSeconds = 45;

    [Header("Current Match Data")]
    [SerializeField, SyncVar] List<BattlePlayer> players = new List<BattlePlayer>();
    public int turnCounter = 1;
    public float gameTimer;
    public Vector3 windDirection;
    public GameState state;

    [SyncVar]
    public BattlePlayer currentPlayersTurn;

    [Header("References")]
    public WorldGenerator worldGen;
    public Transform worldCenterPoint;
    BattlegolfNetworkManager lobby;
    public WorldNetworkCalls worldNetwork;

    [Header("Game Data")]
    public GameObject playerOverviewPrefab;
    public GameObject pawnPrefab;
    public GameObject playerCamPrefab;
    public List<Weapon> weaponList; //TODO: Verify the weapon list to make sure there's no duplicates
    public List<Item> itemPrefabList;
    
    private void Awake()
    {
        instance = this;
        worldGen = FindObjectOfType<WorldGenerator>();
        worldNetwork = GetComponent<WorldNetworkCalls>();
        worldNetwork.worldGen = worldGen;
    }

    private void Start()
    {
        if (NetworkManager.singleton is BattlegolfNetworkManager l)
            lobby = l;
    }

    private void OnEnable()
    {
        WorldGenerator.OnWorldGenerationComplete += WorldGenerationFinished;
    }

    private void OnDisable()
    {
        WorldGenerator.OnWorldGenerationComplete -= WorldGenerationFinished;
    }
    public void InitializeWorld(int numPlayers)
    {
        if (NetworkManager.singleton is BattlegolfNetworkManager l)
            lobby = l;
        state = GameState.WaitingForWorldGeneration;
        worldGen.GenerateWorld(numPlayers * numStartingPawns);
        
    }

    public void ChangeGameState(GameState newState)
    {
        OnChangeFromState(state);
        OnChangeToState(newState);
    }

    void WorldGenerationFinished()
    {
        Debug.Log("worldgen finished");
    }

    public void SpawnPlayer()
    {

    }

    public void OnAllPlayersSpawned()
    {
        Debug.Log("all players spawned");
        SetActivePlayer(players[0]);
        ChangeGameState(GameState.PlayerSelectingPawn);
    }

    public void AddPlayer(BattlePlayer player)
    {
        players.Add(player);
    }

    void OnChangeFromState(GameState oldState)
    {
        switch(oldState)
        {
            case GameState.WaitingForWorldGeneration:
                break;

            case GameState.PlayerSelectingPawn:
                break;

            case GameState.PawnMoving:
                break;

            case GameState.PawnFiring:
                break;

            case GameState.FollowProjectile:
                break;
        }
    }

    void OnChangeToState(GameState newState)
    {
        switch(newState)
        {
            case GameState.WaitingForWorldGeneration:
                break;

            case GameState.PlayerSelectingPawn:
                break;

            case GameState.PawnMoving:
                break;

            case GameState.PawnFiring:
                break;

            case GameState.FollowProjectile:
                break;
        }
    }

    void SetActivePlayer(BattlePlayer player)
    {
        currentPlayersTurn = player;
        player.OnStartTurn();
    }

    void EndTurn()
    {
        turnCounter++;
    }
}
