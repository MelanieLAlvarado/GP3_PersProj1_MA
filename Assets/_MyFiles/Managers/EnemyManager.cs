using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    UIManager _uIManager;
    List<GameObject> _enemies = new List<GameObject>();
    private Transform _enemySpawnLoc; ///<-- recieved from GameManager (may change)
    private GameObject _enemyPrefab;  ///<-- recieved from GameManager (may change)
    [SerializeField] private int enemyAmountToSpawn = 1;
    private void Start()
    {
        _enemySpawnLoc = GameManager.m_Instance.GetEnemySpawnLoc();
        _enemyPrefab = GameManager.m_Instance.GetEnemyPrefab();
        SpawnEnemies();
        StartCoroutine(GatherEnemyManagerDelay());
    }
    private void Update()
    {
        if (_uIManager)
        { 
            CheckEnemyStates();
        }
    }
    private void SpawnEnemies() 
    {
        if (!_enemyPrefab) 
        {
            Debug.LogError("Enemy Prefab not found! Please check the GameManager...");
            return;
        }
        for (int i = 0; i < enemyAmountToSpawn; i++) 
        {
            GameObject enemy;
            if (_enemySpawnLoc != null)
            {
                enemy = Instantiate(_enemyPrefab, _enemySpawnLoc.position, _enemySpawnLoc.rotation);
            }
            else 
            { 
                enemy = Instantiate(_enemyPrefab);
            }
            _enemies.Add(enemy);
        }
    }
    private void CheckEnemyStates() 
    {
        EEnemyState highestState = 0; ///Wait == 0
        foreach (GameObject enemy in _enemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            EEnemyState enemyState = enemyAI.GetEnemyState();
            if (enemyState > highestState && enemyAI.IsTargetPlayer())
            {
                highestState = enemyState;
            }
        }
        _uIManager.UpdateEnemyAwarenessIcon(highestState);
    }
    private IEnumerator GatherEnemyManagerDelay() 
    {
        yield return new WaitForSeconds(0.5f);
        _uIManager = GameManager.m_Instance.GetUIManager();
        if (!_uIManager)
        {
            Debug.LogError("Enemy Manager unable to find UI Manager!");
        }
        StopCoroutine(GatherEnemyManagerDelay());
    }
}
