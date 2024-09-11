using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    NoiseManager noiseManager;
    List<GameObject> enemies = new List<GameObject>();
    private Transform enemySpawnLoc;
    private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 1;//<-- might move into the gamemanager... might leave it as 1 enemy
    private void Start()
    {
        noiseManager = GameManager.m_Instance.GetNoiseManager();
        enemySpawnLoc = GameManager.m_Instance.GetEnemySpawnLoc();
        enemyPrefab = GameManager.m_Instance.GetEnemyPrefab();
        SpawnEnemies();
    }
    private void SpawnEnemies() 
    {
        if (!enemyPrefab) 
        {
            Debug.LogError("Enemy Prefab not found! Please check the GameManager...");
            return;
        }
        for (int i = 0; i < enemyCount; i++) 
        {
            GameObject enemy;
            if (enemySpawnLoc != null)
            {
                enemy = Instantiate(enemyPrefab, enemySpawnLoc.position, enemySpawnLoc.rotation);
            }
            else 
            { 
                enemy = Instantiate(enemyPrefab);
            }
            enemies.Add(enemy);
        }
    }
    public void CheckEnemiesHearingRanges(List<GameObject> triggeredNoiseObjs)
    {
        Debug.Log("Checking all enemies...");
        for (int i = 0; i < enemies.Count; i++)
        {
            Debug.Log(enemies[i]);
            EnemyAI iEnemyAI = enemies[i].GetComponent<EnemyAI>();

            for (int j = 0; j < triggeredNoiseObjs.Count; j++)
            {
                Debug.Log("Checking all noises...");
                Debug.Log(triggeredNoiseObjs[j]);
                Debug.Log(iEnemyAI.GetNoisesObjsInRangeList().Count);
                
                if (iEnemyAI.GetNoisesObjsInRangeList().Find(x => x.ToString() == triggeredNoiseObjs[j].ToString())) 
                {
                    Debug.Log("Is Valid!!! [TEST]");
                }

                if (iEnemyAI.GetNoisesObjsInRangeList().Contains(triggeredNoiseObjs[j])) 
                {
                    iEnemyAI.AddTriggeredNoiseToList(triggeredNoiseObjs[j]);
                }
            }
        }
        //clear/remove noise list objs????
    }
}
