using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    private PlayerControls _playerRef;
    private EnemyManager _enemyManager;
    [SerializeField] private List<GameObject> noiseObjsInScene = new List<GameObject>();
    [SerializeField] private List<GameObject> activeNoiseObjs = new List<GameObject>();

    public void AddNoiseSource(GameObject objToAdd)
    {
        noiseObjsInScene.Add(objToAdd);
    }
    public void AddActiveNoiseSource(GameObject objToAdd)
    {
        if (activeNoiseObjs.Find(x => x.ToString() == objToAdd.ToString()))
        {
            return;//is in list already
        }
        activeNoiseObjs.Add(objToAdd);
    }
    public void ClearActiveNoiseSources() 
    {
        activeNoiseObjs.Clear();
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
        if (GameManager.m_Instance.GetPlayer())
        {
            _playerRef = GameManager.m_Instance.GetPlayer().GetComponent<PlayerControls>();
        }
        if (_playerRef.GetComponent<HearingComponent>()) 
        {
            Debug.Log("PLAYER HAS A HEARING COMPONENT");
            _playerRef.GetComponent<HearingComponent>().CheckHearingRange(activeNoiseObjs);
        }
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
