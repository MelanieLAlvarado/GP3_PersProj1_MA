using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    private EnemyManager _enemyManager;
    [SerializeField] private List<GameObject> noiseObjsInScene = new List<GameObject>();
    [SerializeField] private List<GameObject> activeNoiseObjs = new List<GameObject>();

    public void AddNoiseSource(GameObject objToAdd)
    {
        noiseObjsInScene.Add(objToAdd);
    }
    public void AddActiveNoiseSource(GameObject objToAdd)
    {
        activeNoiseObjs.Add(objToAdd);
    }
    public void ClearActiveNoiseSources() 
    {
        activeNoiseObjs.Clear();
    }
    public bool IsObjInActiveNoiseList(GameObject objToCheck) 
    {
        if (activeNoiseObjs.Contains(objToCheck))
        {
            Debug.Log($"{objToCheck} Is In The NoiseManager!!");
            return true;
        }
        return false;
    }
    private void Start()
    {
        StartCoroutine(GatherEnemyManagerDelay());
    }
    private IEnumerator GatherEnemyManagerDelay()
    {
        yield return new WaitForSeconds(0.25f);
        _enemyManager = GameManager.m_Instance.GetEnemyManager();
        if (_enemyManager != null)
        {
            StopCoroutine(GatherEnemyManagerDelay());
        }
        yield return new WaitForEndOfFrame();
    }

    public void CheckNearbyHearingObjects() 
    {
        Debug.Log("CHECKING HEARNIG!!!");
        //GameManager.m_Instance.GetPlayer().GetComponent<HearingComponent> //check player for hearing component too.
        /*for (int i = 0; i < _NoiseObjsInScene.Count; i++)
        { 
            GameObject obj = _NoiseObjsInScene[i];
            Debug.Log($"{obj}");
        }*/
        _enemyManager.CheckEnemiesHearingRanges(activeNoiseObjs);
    }
    public void RemoveNoise(GameObject objToRemove)
    {
        activeNoiseObjs.Remove(objToRemove);
    }
    public void ClearActiveNoiseList() 
    {
        activeNoiseObjs.Clear();
    }
}
