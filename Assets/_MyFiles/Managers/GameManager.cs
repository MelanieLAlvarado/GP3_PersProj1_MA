using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager m_Instance;

    [Header("Player Info")]///for spawning player and UI
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawn;
    private GameObject _player;
    
    [SerializeField] private GameObject GameplayWidgetPrefab;
    private GameObject _GameplayWidget;

    [Header("Manager Info [READ ONLY]")] ///able to see manager references
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private UIManager uIManager;

    [Header("Enemy Info")]///for spawning Enemies (passed on to enemy Manager; May change later)
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform enemySpawnLoc;
    [SerializeField] private int enemySpawnCount = 1;
    private void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            Debug.LogError("Multiple GameManagers found. Deleting copy...");
            Destroy(this);
        }
        else
        { 
            m_Instance = this;
        }
    }
    public GameObject GetPlayer() { return _player; }
    public GameObject GetEnemyPrefab() { return enemyPrefab; }
    public Transform GetEnemySpawnLoc() { return enemySpawnLoc; }
    public EnemyManager GetEnemyManager() { return enemyManager; }

    public UIManager GetUIManager() { return uIManager; }

    void Start()
    {
        if (playerPrefab && playerSpawn)
        {
            _player = Instantiate(playerPrefab, playerSpawn.position, playerSpawn.rotation);
        }
        //add manager spawns here or earlier
        ClearExtraEnemyManagers();
        CreateEnemyManager();
        CreateGameplayWidget();
    }
    private void CreateEnemyManager()
    {
        if (enemyManager) { return; }
        enemyManager = gameObject.AddComponent<EnemyManager>();
        enemyManager.SetEnemyCount(enemySpawnCount);
    }
    private void CreateGameplayWidget()
    {
        if (!GameplayWidgetPrefab) 
        {
            Debug.LogError("No Gameplay Widget Prefab! Please add in GameManager...");
            return; 
        }
        if (!_GameplayWidget)
        {
            _GameplayWidget = Instantiate(GameplayWidgetPrefab);
        }
        if (uIManager) { return; }

        uIManager = _GameplayWidget.GetComponent<UIManager>();
    }
    private void ClearExtraEnemyManagers()
    {
        EnemyManager[] enemyManagerList = this.GetComponents<EnemyManager>();
        if (enemyManagerList.Length > 0)
        {
            Debug.LogError("Multiple EnemyManagers found. Destroying copy...");
            foreach (EnemyManager enemyManager in enemyManagerList)
            {
                Destroy(enemyManager);
            }
        }
    }
}
public enum EEntityType { None, Player, Enemy, NoiseMaker, HidingSpace}

