using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class HearingComponent : MonoBehaviour
{
    //WIP rework to function on player
    private NoiseManager _noiseManager;
    private UIManager _uIManager;
    [Header("Hearing Options")]
    [Range(1.0f, 50.0f)][SerializeField] float hearingRange = 30.0f;
    [Range(0f, 100f)][SerializeField] float hearingThreshold = 20.0f;
    private bool _areNoisesInaudible = true;
    private bool _canDetectSelf = false;

    [Header("Hearing List Info [READ ONLY]")]
    [SerializeField] private List<GameObject> noiseObjsInRange = new List<GameObject>();
    [SerializeField] private List<GameObject> audibleNoiseList = new List<GameObject>();
    [SerializeField] private List<float> noiseCalculatedValues = new List<float>();
    private float targetNoiseCalculatedValue = 0.0f;

    public bool GetAreNoisesInaudible() { return _areNoisesInaudible; }
    public List<GameObject> GetNoisesObjsInRangeList() { return noiseObjsInRange; }
    public List<float> GetNoiseCalculatedValues() { return noiseCalculatedValues; }
    public bool GetIsAudibleNoisesPresent() 
    {
        return audibleNoiseList.Count > 0; 
    }
    public void SetCanDetectSelf(bool detectToSet) 
    {
        _canDetectSelf = detectToSet;
    }
    public void SetHearingThreshold(float amountToSet) 
    {
        hearingThreshold = amountToSet;
    }
    public void CheckHearingRange(List<GameObject> listToCheck) 
    {
        for (int j = 0; j < listToCheck.Count; j++)
        {
            Debug.Log("OBJECT CHECK");
            if (noiseObjsInRange.Find(x => x.ToString() == listToCheck[j].ToString()))
            {
                Debug.Log("Is Valid!!!               [TEST]");
                AddToAudibleNoiseList(listToCheck[j]);
            }
        }
    }
    public void UpdateNoiseMeter() //used by player. may move later
    {
        _uIManager = GameManager.m_Instance.GetUIManager();
        _uIManager.UpdateNoiseMeterUI(targetNoiseCalculatedValue);
    }
    private void Start()
    {
        _noiseManager = GameManager.m_Instance.GetNoiseManager();

        SphereCollider colliderRange = GetComponent<SphereCollider>();
        colliderRange.isTrigger = true;
        colliderRange.radius = hearingRange;
        LayerMask visualMask = GameManager.m_Instance.GetVisualMask();
        Debug.Log("added layer");
        colliderRange.excludeLayers += visualMask;

        if (this.gameObject.GetComponent<EnemyAI>())
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }
        if (this.gameObject.GetComponent<NoiseComponent>() && _canDetectSelf == true)
        {
            noiseObjsInRange.Add(this.gameObject);
        }
    }
    private void AddToAudibleNoiseList(GameObject noiseToAdd)
    {
        if (audibleNoiseList.Contains(noiseToAdd))
        {
            Debug.Log("This obj is already in list!");
            return;
        }
        if (CalculateSingleNoiseValue(noiseToAdd) >= hearingThreshold)
        {
            Debug.Log("Noise has been added...");
            audibleNoiseList.Add(noiseToAdd);
        }
        else
        {
            Debug.Log("Noise is inaudible! wasn't added to list.");
            _noiseManager.RemoveNoise(noiseToAdd);
        }
    }
    private float CalculateSingleNoiseValue(GameObject objToReceive) 
    {
        float iDistance = Vector3.Distance(transform.position, objToReceive.transform.position);

        float distMultiplier = 1 - (iDistance / hearingRange);

        NoiseComponent noiseComp = objToReceive.GetComponent<NoiseComponent>();
        if (objToReceive == GameManager.m_Instance.GetPlayer())
        {
            Debug.Log("PLayer noise val is:{{{" + noiseComp.GetRawNoiseAmount() * distMultiplier);
        }
        return noiseComp.GetRawNoiseAmount() * distMultiplier;
    }

    private void CalculateNoiseValues()
    {
        if (audibleNoiseList.Count > 0)
        {
            for (int i = 0; i < audibleNoiseList.Count; i++)
            {
                float noiseCalcVal = CalculateSingleNoiseValue(audibleNoiseList[i]);
                noiseCalculatedValues.Add(noiseCalcVal);
            }
        }
    }
    public Transform ChooseNoiseTarget()
    {
        //iterates through the list to find the highest noise value
        CalculateNoiseValues();
        int index = 0;
        float noiseNum = noiseCalculatedValues[index];
        Transform targetPos = audibleNoiseList[index].transform;
        for (int i = 1; i < audibleNoiseList.Count; i++) ///choose the sound with the highest noise
        {
            float iNoiseNum = noiseCalculatedValues[i];
            if (noiseNum <= iNoiseNum)
            {
                noiseNum = iNoiseNum;
                index = i;
            }
        }
        targetPos = audibleNoiseList[index].transform;
        //Debug.Log($"noiseNum == {noiseNum}!");
        _noiseManager.ClearActiveNoiseList();

        targetNoiseCalculatedValue = noiseCalculatedValues[index];
        noiseCalculatedValues.Clear();
        if (noiseNum > hearingThreshold)
        {
            _areNoisesInaudible = false;
            return targetPos;
        }
        targetNoiseCalculatedValue = 0.0f;
        _areNoisesInaudible = true;
        return null;
    }
    public void ClearAudibleLists() 
    {
        audibleNoiseList.Clear();
        noiseCalculatedValues.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        ///for when hearing range touches a hearable object
        if (other.gameObject.GetComponent<NoiseComponent>() && !noiseObjsInRange.Contains(other.gameObject))
        {
            noiseObjsInRange.Add(other.gameObject);
        }
    }
    //.Find(x => x.ToString() == triggeredNoiseObjs[j].ToString())
    private void OnTriggerExit(Collider other)
    {
        ///for when hearing range leaves a hearable object
        if (other.gameObject.GetComponent<NoiseComponent>() && !other.isTrigger)
        {
            noiseObjsInRange.Remove(other.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
