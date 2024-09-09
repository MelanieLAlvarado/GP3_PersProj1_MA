using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    private EnemyManager enemyManager;
    private List<GameObject> noiseObjects = new List<GameObject>();
    //[SerializeField] private List<GameObject> _audibleNoiseList = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(GatherEnemyManagerDelay());
    }
    private IEnumerator GatherEnemyManagerDelay()
    {
        yield return new WaitForSeconds(0.5f);
        enemyManager = GameManager.m_Instance.GetEnemyManager();
        if (enemyManager != null)
        {
            StopCoroutine(GatherEnemyManagerDelay());
        }
        yield return new WaitForEndOfFrame();
    }
    public void AddNoise(GameObject objToAdd) 
    {
        Debug.Log("Noise added to NoiseManager!");
        noiseObjects.Add(objToAdd);
        enemyManager.CheckEnemiesHearingRanges();
    }
    /*public void RemoveNoise(GameObject objToRemove)
    {
        noiseObjects.Remove(objToRemove);
        enemyManager.CheckEnemiesHearingRanges();
    }*/
    public void CheckNoiseInEnemyRange(EnemyAI enemyToCheck)
    {
        Debug.Log("Checking all noises...");

        for (int i = 0; i < noiseObjects.Count; i++)
        {
            enemyToCheck.HearingCheck(noiseObjects[i]);
        }
    }
}
