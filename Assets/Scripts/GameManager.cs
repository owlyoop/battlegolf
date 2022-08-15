using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
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
    [SerializeField] List<BattlePlayer> players = new List<BattlePlayer>();
    public int turnCounter = 1;
    public float gameTimer;
    public Vector3 windDirection;

    BattlePlayer currentPlayersTurn;
    BattlePlayer activePlayer;

    [Header("References")]
    public WorldGenerator worldGen;
    public Transform worldCenterPoint;
    BattlegolfNetworkManager lobby;

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
    }

    private void Start()
    {
        if (NetworkManager.singleton is BattlegolfNetworkManager l)
            lobby = l;
    }

    public void InitializeWorld()
    {
        worldGen.GenerateWorld();
        lobby.SpawnBattlePlayers();
        PopulateWorldWithPawns(players.Count, numStartingPawns);
    }

    /*public Player GetPlayer(NetworkIdentity id)
    {
        Player foundPlayer = null;
        if (NetworkManager.singleton is BattlegolfNetworkManager lobby)
        {
            foreach (var player in lobby.players)
            {
                if (id == player.netIdentity)
                {
                    foundPlayer = player.battlePlayerObj;
                }
            }
        }

        return foundPlayer;
    }*/

    void SetActivePlayer(BattlePlayer player)
    {

    }

    void PopulateWorldWithPawns(int numPlayers, int pawnsPerPlayer)
    {
        worldGen.GeneratePawnSpawnpoints(numPlayers * pawnsPerPlayer);

        for (int n = 0; n < numPlayers; n++)
        {
            for (int p = 0; p < pawnsPerPlayer; p++)
            {
                Debug.Log(players.Count);
                Debug.Log(players[n].ToString());
                Debug.Log(worldGen.pawnSpawns[(n + 1 * p + 1) - 1]);
                AddPawn(players[n], worldGen.pawnSpawns[(n+1 * p+1) - 1]);
            }
        }

    }


    void AddPawn(BattlePlayer owner, Vector3 position)
    {
        Debug.Log("pawn spawned");
        GameObject go = Instantiate(pawnPrefab, position, Quaternion.identity);
        Pawn pawn = go.GetComponent<Pawn>();

        pawn.owner = owner;
        owner.ownedPawns.Add(pawn);
        owner.selectedPawn = pawn;

        NetworkServer.Spawn(go);
    }
}
