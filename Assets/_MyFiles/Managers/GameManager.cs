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
    [SerializeField] private NoiseManager noiseManager;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private UIManager uIManager;

    [Header("Enemy Info")]///for spawning Enemies (passed on to enemy Manager; May change later)
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform enemySpawnLoc;

    [Header("Other Info/Options")] /// for visual (may remove or change)
    [SerializeField] private LayerMask visualMask;
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
    public NoiseManager GetNoiseManager() { return noiseManager; }
    public EnemyManager GetEnemyManager() { return enemyManager; }

    public UIManager GetUIManager() { return uIManager; }
    public LayerMask GetVisualMask() {  return visualMask; }

    void Start()
    {
        if (playerPrefab && playerSpawn)
        {
            _player = Instantiate(playerPrefab, playerSpawn.position, playerSpawn.rotation);
        }
        //add manager spawns here or earlier
        ClearExtraManagers();
        CreateNoiseManager();
        CreateEnemyManager();
        CreateGameplayWidget();
    }
    private void CreateNoiseManager()
    {
        if (noiseManager) { return; }
        noiseManager = gameObject.AddComponent<NoiseManager>();
    }
    private void CreateEnemyManager()
    {
        if (enemyManager) { return; }
        enemyManager = gameObject.AddComponent<EnemyManager>();
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
    private void ClearExtraManagers()
    {
        NoiseManager[] noiseManagerList = this.GetComponents<NoiseManager>();
        if (noiseManagerList.Length > 0)
        {
            Debug.LogError("Multiple NoiseManagers found. Destroying copy...");
            foreach (NoiseManager noiseManager in noiseManagerList)
            {
                Destroy(noiseManager);
            }
        }
        EnemyManager[] enemyManagerList = this.GetComponents<EnemyManager>();
        if (enemyManagerList.Length > 0)
        {
            Debug.LogError("Multiple EnemyManagers found. Destroying copy...");
            foreach (EnemyManager enemyManager in enemyManagerList)
            {
                Destroy(enemyManager);
            }
        }
        //Will change later
        UIManager[] uIManagerList = this.GetComponents<UIManager>();
        if (uIManagerList.Length > 0)
        {
            Debug.LogError("Multiple UIManagers found. Destroying copy...");
            foreach (UIManager uIManager in uIManagerList)
            {
                Destroy(uIManager);
            }
        }
    }
}
public enum EEntityType { None, Player, Enemy, NoiseMaker, HidingSpace}

