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
        Debug.Log("Checking all enemies...");
        for (int i = 0; i < _enemies.Count; i++)
        {
            Debug.Log(_enemies[i]);
            EnemyAI iEnemyAI = _enemies[i].GetComponent<EnemyAI>();

            HearingComponent enemyHearingComp = _enemies[i].GetComponent<HearingComponent>();

            for (int j = 0; j < triggeredNoiseObjs.Count; j++)
            {
                Debug.Log("Checking all noises...");
                Debug.Log(triggeredNoiseObjs[j]);
                Debug.Log(enemyHearingComp.GetNoisesObjsInRangeList().Count);

                ///~~~REWORK BEGAN~~~
                if (enemyHearingComp.GetNoisesObjsInRangeList().Find(x => x.ToString() == triggeredNoiseObjs[j].ToString()))
                {
                    Debug.Log("Is Valid!!! [TEST]");
                }
                if (enemyHearingComp.GetNoisesObjsInRangeList().Contains(triggeredNoiseObjs[j]))
                {
                    enemyHearingComp.AddTriggeredNoiseToList(triggeredNoiseObjs[j]);
                }
                ///~~~REWORK END~~~
            }
        }
        //clear/remove noise list objs????
    }
}
