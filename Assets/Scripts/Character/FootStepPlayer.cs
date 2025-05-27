using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(FMODUnity.StudioEventEmitter))]
public class FootStepPlayer : MonoBehaviour
{
    private StudioEventEmitter _emitter;
    
    private void Awake()
    {
        _emitter = GetComponent<StudioEventEmitter>();
    }

    private void PlayFootstep() //Used by animation events
    {
        _emitter.Play();
    }
}
