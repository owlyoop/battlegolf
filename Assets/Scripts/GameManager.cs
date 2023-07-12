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
    public int NumPlayers = 1;
    public int NumStartingPawns = 4;
    public float TurnLengthInSeconds = 45;

    [Header("Current Match Data")]
    [SerializeField, SyncVar] List<BattlePlayer> players = new List<BattlePlayer>();
    public int TurnCounter = 1;
    public float GameTimer;
    public Vector3 WindDirection;
    public GameState State;

    [SyncVar]
    public BattlePlayer CurrentPlayersTurn;

    [Header("References")]
    public WorldGenerator WorldGen;
    public Transform WorldCenterPoint;
    BattlegolfNetworkManager lobby;
    public WorldNetworkCalls WorldNetwork;

    [Header("Game Data")]
    public GameObject PlayerOverviewPrefab;
    public GameObject PawnPrefab;
    public GameObject PlayerCamPrefab;
    public List<Weapon> WeaponList; //TODO: Verify the weapon list to make sure there's no duplicates
    public List<Item> ItemPrefabList;
    
    private void Awake()
    {
        instance = this;
        WorldGen = FindObjectOfType<WorldGenerator>();
        WorldNetwork = GetComponent<WorldNetworkCalls>();
        WorldNetwork.worldGen = WorldGen;
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
        State = GameState.WaitingForWorldGeneration;
        WorldGen.GenerateWorld(numPlayers * NumStartingPawns);
        
    }

    public void ChangeGameState(GameState newState)
    {
        OnChangeFromState(State);
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
        CurrentPlayersTurn = player;
        player.OnStartTurn();
    }

    void EndTurn()
    {
        TurnCounter++;
    }
}
