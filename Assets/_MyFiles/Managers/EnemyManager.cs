using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    List<GameObject> _enemies = new List<GameObject>();
    private Transform _enemySpawnLoc;
    private GameObject _enemyPrefab;
    [SerializeField] private int enemyAmountToSpawn = 1;//<-- might move into the gamemanager... might leave it as 1 enemy
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
    public void CheckEnemiesHearingRanges(List<GameObject> triggeredNoiseObjs)
    {
        ///Checking every enemy in manager
        Debug.Log("Chgeckgibns ing if lsit");
        for (int i = 0; i < _enemies.Count; i++)
        {
            Debug.Log("ENEMY CHECK");
            EnemyAI iEnemyAI = _enemies[i].GetComponent<EnemyAI>();

            HearingComponent enemyHearingComp = _enemies[i].GetComponent<HearingComponent>();
            ///Checking every object from parameters
            for (int j = 0; j < triggeredNoiseObjs.Count; j++)
            {
                Debug.Log("OBJECT CHECK");
                if (enemyHearingComp.GetNoisesObjsInRangeList().Find(x => x.ToString() == triggeredNoiseObjs[j].ToString()))
                {
                    Debug.Log("Is Valid!!!               [TEST]");
                    enemyHearingComp.AddToAudibleNoiseList(triggeredNoiseObjs[j]);
                }
            }
        }
    }
}
