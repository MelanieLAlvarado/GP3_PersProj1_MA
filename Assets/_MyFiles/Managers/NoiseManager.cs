using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    private EnemyManager enemyManager;
    private List<GameObject> activeNoiseObjs = new List<GameObject>();
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
    public void AddActiveNoiseSource(GameObject objToAdd) 
    {
        Debug.Log("Noise added to NoiseManager!");
        activeNoiseObjs.Add(objToAdd);
        enemyManager.CheckEnemiesHearingRanges(activeNoiseObjs);
    }
    public void RemoveNoise(GameObject objToRemove)
    {
        activeNoiseObjs.Remove(objToRemove);
    }
}
