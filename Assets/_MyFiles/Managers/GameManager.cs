using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager m_Instance;

    [Header("Player Info")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawn;
    private GameObject _player;
    
    [Header("Manager Info [READ ONLY]")]
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private UIManager uIManager;
    //[SerializeField] private NoiseManager noiseManager;
    private void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            Debug.LogError("Multiple GameManagers found. Deleting copy...");
            Destroy(this);
        }
    }

    void Start()
    {
        if (playerPrefab && playerSpawn)
        {
            _player = Instantiate(playerPrefab, playerSpawn.position, playerSpawn.rotation);
        }
        //add manager spawns here or earlier
        CreateEnemyManager();
        CreateUIManager();
        DestroyExtraManagers();
    }

    private void CreateEnemyManager()
    {
        if (enemyManager) { return; }
        enemyManager = gameObject.AddComponent<EnemyManager>();
    }

    private void CreateUIManager()
    {
        if (uIManager) { return; }

        uIManager = gameObject.AddComponent<UIManager>();
    }

    public GameObject GetPlayer() { return _player; }
    
    public EnemyManager GetEnemyManager() { return enemyManager; }
    
    public UIManager GetUIManager() { return uIManager; }
    private void DestroyExtraManagers()//WIP to make nicer
    {
        if (this.GetComponent<EnemyManager>() != enemyManager)
        {
            Debug.LogError("Multiple EnemyManagers found. Destroying copy...");
            Destroy(this.GetComponent<EnemyManager>());
        }
        if (this.GetComponent<UIManager>() != uIManager)
        {
            Debug.LogError("Multiple UIManagers found. Destroying copy...");
            Destroy(this.GetComponent<UIManager>());
        }
    }
}

public enum EEntityType { None, Player, Enemy, NoiseMaker, HidingSpace}