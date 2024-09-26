using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SphereCollider))]
public class HearingComponent : Sense
{
    private UIManager _uIManager;
    [Header("Hearing Options")]
    [Range(1.0f, 50.0f)][SerializeField] float hearingRange = 30.0f;
    [Range(0f, 100f)][SerializeField] float hearingThreshold = 20.0f;
    private bool _bAreNoisesInaudible = true;
    private bool _bCanDetectSelf = false;//May be obsolete

    [Header("Hearing List Info [READ ONLY]")]
    [SerializeField] private List<GameObject> audibleNoiseList = new List<GameObject>();
    [SerializeField] private List<float> noiseCalculatedValues = new List<float>();
    private float targetNoiseCalculatedValue = 0.0f;

    public bool GetAreNoisesInaudible() { return _bAreNoisesInaudible; }
    public List<float> GetNoiseCalculatedValues() { return noiseCalculatedValues; }
    public bool GetIsAudibleNoisesPresent() ///usually starts the hearing process in other scripts
    {
        return audibleNoiseList.Count > 0; 
    }
    public void SetCanDetectSelf(bool detectToSet) 
    {
        _bCanDetectSelf = detectToSet;
    }
    public void SetHearingThreshold(float amountToSet) 
    {
        hearingThreshold = amountToSet;
    }
    /*public void CheckHearingRange(List<GameObject> soundsToCheck) 
    {
        for (int j = 0; j < soundsToCheck.Count; j++)
        {
            Debug.Log("OBJECT CHECK");
            if (IsInHearingRange(soundsToCheck[j].transform))
            {
                AddToAudibleNoiseList(soundsToCheck[j]);
            }
        }
    }*/

    public void RemoveFromAudibleNoiseList(GameObject noiseToRemove)
    {
        if (!audibleNoiseList.Contains(noiseToRemove))
        {
            return;
        }
        audibleNoiseList.Remove(noiseToRemove);
    }
    public void UpdateNoiseMeter() ///used by player. may move later
    {
        _uIManager = GameManager.m_Instance.GetUIManager();
        _uIManager.UpdateNoiseMeterUI(targetNoiseCalculatedValue);
    }
    private void Start()
    {
        SphereCollider colliderRange = GetComponent<SphereCollider>();
        colliderRange.isTrigger = true;
        colliderRange.radius = hearingRange;
        LayerMask visualMask = GameManager.m_Instance.GetVisualMask();
        colliderRange.excludeLayers += visualMask;

        if (this.gameObject.GetComponent<EnemyAI>()) 
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }
        /*if (this.gameObject.GetComponent<NoiseComponent>() && _canDetectSelf == true) 
        {
            noiseObjsInRange.Add(this.gameObject);
        }*/
    }
    //Private update where all stimuli(sense) values are checked and added to the hearing component
    private void Update()
    {
        foreach (Stimuli stimuli in GetCurrentSensibleStimuliSet())
        {
            AddToAudibleNoiseList(stimuli.gameObject);
            Debug.Log(GetCurrentSensibleStimuliSet().Count);

            /*NoiseComponent noise = stimuli.gameObject.GetComponent<NoiseComponent>();
            if (noise != null && !noise.GetIsTriggered())
            {
                RemoveFromAudibleNoiseList(stimuli.gameObject);
            }*/
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

            if (noiseToAdd.GetComponent<NoiseComponent>().GetIsTriggered() == true) 
            {
                audibleNoiseList.Add(noiseToAdd);
            }
        }
        Debug.Log("Noise is inaudible! wasn't added to list.");
    }
    private float CalculateSingleNoiseValue(GameObject objToReceive)
    {
        float iDistance = Vector3.Distance(transform.position, objToReceive.transform.position);

        float distMultiplier = 1 - (iDistance / hearingRange); ///multiplier based on distance in range

        NoiseComponent noiseComp = objToReceive.GetComponent<NoiseComponent>();

        return noiseComp.GetRawNoiseAmount() * distMultiplier;
    }

    private void CalculateNoiseValues() ///saving each noise value
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
        ///iterates through the list to find the highest noise value
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
        //_noiseManager.ClearActiveNoiseList();

        targetNoiseCalculatedValue = noiseCalculatedValues[index];
        noiseCalculatedValues.Clear();
        if (noiseNum > hearingThreshold)
        {
            _bAreNoisesInaudible = false;
            return targetPos;
        }
        targetNoiseCalculatedValue = 0.0f;
        _bAreNoisesInaudible = true;
        return null;
    }
    public void ClearAudibleLists() ///reseting the sounds heard
    {
        audibleNoiseList.Clear();
        noiseCalculatedValues.Clear();
    }

    /*private bool IsInHearingRange(Transform otherObject) 
    {
        float distanceFromOwner = Vector3.Distance(transform.position, otherObject.position);
        if (distanceFromOwner < hearingRange)
        {
            return true;
        }
        return false;
    }*/

    protected override void OnDrawDebug()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }

    protected override bool IsStimuliSensible(Stimuli stimuli)
    {
        return transform.InRangeOf(stimuli.transform, hearingRange);
    }
}
