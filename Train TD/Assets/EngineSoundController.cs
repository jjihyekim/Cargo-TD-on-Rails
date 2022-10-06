using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineSoundController : MonoBehaviour {
    private AudioSource _source;
    private ISpeedForEngineSoundsProvider _provider;
    
    void Start() {
        _source = GetComponent<AudioSource>();
        _provider = GetComponentInParent<ISpeedForEngineSoundsProvider>();
        Invoke(nameof(RandomPitchChange), Random.Range(3f,10f));
    }


    public float naturalPitch = 1f;
    public float targetPitch = 1f;
    public float pitchMultiplier = 1f;
    void Update() {
        
        targetPitch = _provider.GetSpeed().Remap(0, 10, 0.5f, 1.5f);
        
        _source.pitch = Mathf.Lerp(_source.pitch, naturalPitch * targetPitch * pitchMultiplier, 1f*Time.deltaTime);
    }

    void RandomPitchChange() {
        pitchMultiplier = 1.1f;
        Invoke(nameof(RevertRandomPitchChange), Random.Range(0.1f,0.2f));
    }

    void RevertRandomPitchChange() {
        pitchMultiplier = 1f;
        Invoke(nameof(RandomPitchChange), Random.Range(3f,10f));
    }
}

public interface ISpeedForEngineSoundsProvider {
    public float GetSpeed();
}
