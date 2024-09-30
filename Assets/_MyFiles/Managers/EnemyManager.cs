using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    List<GameObject> _enemies = new List<GameObject>();
    private Transform _enemySpawnLoc; ///<-- recieved from GameManager (may change)
    private GameObject _enemyPrefab;  ///<-- recieved from GameManager (may change)
    [SerializeField] private int enemyAmountToSpawn = 1;
    private void Start()
    {
        _enemySpawnLoc = GameManager.m_Instance.GetEnemySpawnLoc();
        _enemyPrefab = GameManager.m_Instance.GetEnemyPrefab();
        SpawnEnemies();
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
}
