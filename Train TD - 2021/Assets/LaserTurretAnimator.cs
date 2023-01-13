using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurretAnimator : MonoBehaviour
{
    private GunModule _gunModule;

    private float warmUpTime;

    public AudioClip warmUpClip;
    public AudioClip stopClip;
    
    
    public AudioSource introAudioSource;
    public AudioSource loopAudioSource;

    public GeroBeam myBeam;
    void PlayGunShoot () {
        introAudioSource.clip = warmUpClip;
        introAudioSource.Play();
        loopAudioSource.PlayDelayed(warmUpTime);
    }
    // Start is called before the first frame update
    void Start() {
        _gunModule = GetComponentInParent<GunModule>();
        
        if (_gunModule == null) {
            Debug.LogError("Can't find GunModule!");
            this.enabled = false;
            return;
        }
        
        _gunModule.startWarmUpEvent.AddListener(OnWarmUp);
        _gunModule.stopShootingEvent.AddListener(OnStopShooting);
    }

    void OnWarmUp() {
        warmUpTime = _gunModule.GetFireDelay();
        PlayGunShoot();
        myBeam.ActivateBeam();
    }

    void OnStopShooting() {
        introAudioSource.Stop();
        loopAudioSource.Stop();
        introAudioSource.PlayOneShot(stopClip);
        
        myBeam.DisableBeam();
    }
}
