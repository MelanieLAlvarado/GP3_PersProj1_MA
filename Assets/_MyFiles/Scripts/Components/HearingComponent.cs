using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]

public class HearingComponent : MonoBehaviour
{
    //WIP rework to function on player
    [Range(1.0f, 30.0f)][SerializeField] float hearingRange = 15f;
    [Range(0f, 100f)][SerializeField] float hearingThreshold = 40f;
    //[SerializeField] private GameObject _hRange; ///visual of hearing range (temp)

    [SerializeField] private List<GameObject> noiseObjsInRange = new List<GameObject>();
    [SerializeField] private List<GameObject> audibleNoiseList = new List<GameObject>();
    [SerializeField] private List<float> noiseCalculatedValues = new List<float>();
    public List<GameObject> GetNoisesObjsInRangeList() { return noiseObjsInRange; }
    public bool GetIsAudibleNoisesPresent() { return audibleNoiseList.Count != 0; }
    public List<float> GetNoiseCalculatedValues() { return noiseCalculatedValues; }

    public void AddTriggeredNoiseToList(GameObject noiseToAdd)
    {
        if (audibleNoiseList.Contains(noiseToAdd))
        {
            Debug.Log("This obj is already in list!");
            return;
        }
        Debug.Log("Noise has been added...");
        audibleNoiseList.Add(noiseToAdd);
    }


    private void Start()
    {
        SphereCollider colliderRange= GetComponent<SphereCollider>();
        colliderRange.isTrigger = true;
        colliderRange.radius = hearingRange;
    }

    private float CalculateSingleSoundValue(GameObject objToReceive) 
    {
        float iDistance = Vector3.Distance(transform.position, objToReceive.transform.position);

        float distMultiplier = iDistance / hearingRange;

        NoiseComponent noiseComp = objToReceive.GetComponent<NoiseComponent>();
        return noiseComp.GetRawSoundAmount() * distMultiplier;
    }

    private void CalculateSoundValues() // separate hearing range on component instead??
    {
        if (audibleNoiseList.Count > 0) //might find a more understandable way to gauge sound value later...
        {
            for (int i = 0; i < audibleNoiseList.Count; i++) //calculate sound values
            {
                float noiseCalcVal = CalculateSingleSoundValue(audibleNoiseList[i]);
                noiseCalculatedValues.Add(noiseCalcVal);
            }
        }
        //Choose noise target
    }
    private Transform ChooseNoiseTarget()
    {
        //iterates through the list to find the highest noise value
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
            targetPos = audibleNoiseList[index].transform;
        }
        return targetPos;
    }
    public void ClearLists() 
    {
        audibleNoiseList.Clear();
        noiseCalculatedValues.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        ///for when hearing range touches a hearable object
        Debug.Log(other);
        if (other.gameObject.GetComponent<INoiseInteraction>())
        {
            Debug.Log($"{this.gameObject.name}: near noisemaker");
            noiseObjsInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ///for when hearing range leaves a hearable object
        Debug.Log(other);
        if (other.gameObject.GetComponent<INoiseInteraction>())
        {
            Debug.Log($"{this.gameObject.name}: lost noisemaker");
            noiseObjsInRange.Remove(other.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
