using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    private EnemyManager _enemyManager;
    private List<GameObject> _activeNoiseObjs = new List<GameObject>();
    //[SerializeField] private List<GameObject> _audibleNoiseList = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(GatherEnemyManagerDelay());
    }
    private IEnumerator GatherEnemyManagerDelay()
    {
        yield return new WaitForSeconds(0.5f);
        _enemyManager = GameManager.m_Instance.GetEnemyManager();
        if (_enemyManager != null)
        {
            StopCoroutine(GatherEnemyManagerDelay());
        }
        yield return new WaitForEndOfFrame();
    }
    public void AddActiveNoiseSource(GameObject objToAdd) 
    {
        Debug.Log("Noise added to NoiseManager!");
        _activeNoiseObjs.Add(objToAdd);
        //GameManager.m_Instance.GetPlayer().GetComponent<HearingComponent> //check player for hearing component too.
        _enemyManager.CheckEnemiesHearingRanges(_activeNoiseObjs);
    }
    public void RemoveNoise(GameObject objToRemove)
    {
        _activeNoiseObjs.Remove(objToRemove);
    }
}
