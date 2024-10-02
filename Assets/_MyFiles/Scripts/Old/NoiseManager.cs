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

    public void AddNoiseSourceInScene(GameObject objToAdd)
    {
        noiseObjsInScene.Add(objToAdd);
    }
    public void AddActiveNoiseSource(GameObject objToAdd)
    {
        if (activeNoiseObjs.Find(x => x.ToString() == objToAdd.ToString()))
        {
            return;///is in list already. no duplicates allowed
        }
        activeNoiseObjs.Add(objToAdd);
    }
    public void ClearActiveNoiseSources() ///noises are done
    {
        activeNoiseObjs.Clear();
    }
    private void Start()
    {
        //StartCoroutine(GatherEnemyManagerDelay());
    }
    private IEnumerator GatherEnemyManagerDelay() ///delay until enemy manager exists
    {
        yield return new WaitForSeconds(0.25f);
        _enemyManager = GameManager.m_Instance.GetEnemyManager();
        if (_enemyManager != null)
        {
            //StopCoroutine(GatherEnemyManagerDelay());
        }
        yield return new WaitForEndOfFrame();
    }

    public void CheckNearbyHearingObjects() ///passes activeNoise objects into hearing components
    {
        if (GameManager.m_Instance.GetPlayer())
        {
            _playerRef = GameManager.m_Instance.GetPlayer().GetComponent<PlayerControls>();
        }
        if (_playerRef.GetComponent<HearingComponent>()) 
        {
            //_playerRef.GetComponent<HearingComponent>().CheckHearingRange(activeNoiseObjs);
        }
        //_enemyManager.CheckEnemiesHearingRanges(activeNoiseObjs); ///passing into enemy manager to check enemies
    }
    public void RemoveActiveNoise(GameObject objToRemove)
    {
        activeNoiseObjs.Remove(objToRemove);
    }
    public void ClearActiveNoiseList() 
    {
        activeNoiseObjs.Clear();
    }
}
