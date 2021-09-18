using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    private float curSpeed = 0;
    private float curSeekStrength = 0;
    public float acceleration = 5f;
    public float seekAcceleration = 200f;
    public float speed = 5f;
    public float damage = 20f;

    public float lifetime = 20f;
    
    public GameObject myOriginObject;

    public bool isTargetSeeking = true;
    public float seekStrength = 180f;
    public Transform target;

    
    public enum HitType {
        Bullet, Rocket
    }

    public HitType myHitType = HitType.Bullet;
    private void Start() {
        Invoke("DestroySelf", lifetime);
        if (myHitType != HitType.Rocket) {
            curSpeed = speed;
            curSeekStrength = seekStrength;
        }
    }

    void DestroySelf() {
        Destroy(gameObject);
    }

    void Update() {
        if (isTargetSeeking) {
            if (target != null) {
                var targetLook = Quaternion.LookRotation(target.position - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetLook, curSeekStrength * Time.deltaTime);
            } else {
                isTargetSeeking = false;
            }
        }

        if (myHitType == HitType.Rocket) {
            curSpeed = Mathf.MoveTowards(curSpeed, speed, acceleration * Time.deltaTime);
            curSeekStrength = Mathf.MoveTowards(curSeekStrength, seekStrength, seekAcceleration * Time.deltaTime);
        }

        if (target != null) {
            if (Vector3.Distance(transform.position, target.position) < curSpeed * Time.deltaTime) {
                DestroyFlying();
            }
        }

        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * curSpeed * Time.deltaTime);
    }

    void SmartDestroySelf() {

        var particles = GetComponentsInChildren<ParticleSystem>();

        foreach (var particle in particles) {
            particle.transform.SetParent(null);
            particle.Stop();
            Destroy(particle.gameObject, 5f);
        }
        
        Destroy(gameObject);
    }

    private void DestroyFlying() {
        //print("destroyflying");
        GameObject hitPrefab = null;
        switch (myHitType) {
            case HitType.Bullet:
                hitPrefab = LevelReferences.s.metalBulletHitEffectPrefab;
                break;
            case HitType.Rocket:
                hitPrefab = LevelReferences.s.rocketExplosionEffectPrefab;
                break;
        }

        var health = target.GetComponentInParent<IHealth>();

        if (health != null) {
            health.DealDamage(damage);
        }

        Instantiate(hitPrefab, transform.position, transform.rotation);
        SmartDestroySelf();
    }


    private void OnCollisionEnter(Collision other) {
        if (other.transform.root.gameObject != myOriginObject) {
            switch (myHitType) {
                case HitType.Bullet:
                    ContactDamage(other);
                    break;
                case HitType.Rocket:
                    ExplosiveDamage(other);
                    break;
            }

            SmartDestroySelf();
        }
    }

    private void ExplosiveDamage(Collision other) {
        var contact = other.GetContact(0);
        var pos = contact.point;
        var rotation = Quaternion.LookRotation(contact.normal);

        GameObject hitPrefab = LevelReferences.s.rocketExplosionEffectPrefab;

        var health = other.gameObject.GetComponentInParent<IHealth>();
        //print(other.gameObject);
        //print(other.transform.root.gameObject);
        //print(health);
        if (health != null) {
            health.DealDamage(damage);
        }

        Instantiate(hitPrefab, pos, rotation);
    }

    private void ContactDamage(Collision other) {
        var contact = other.GetContact(0);
        var pos = contact.point;
        var rotation = Quaternion.LookRotation(contact.normal);

        GameObject hitPrefab;
        if (other.gameObject.GetComponentInParent<Cart>()) {
            hitPrefab = LevelReferences.s.metalBulletHitEffectPrefab;
        } else {
            hitPrefab = LevelReferences.s.dirtBulletHitEffectPrefab;
        }

        var health = other.gameObject.GetComponentInParent<IHealth>();
        //print(other.gameObject);
        //print(other.transform.root.gameObject);
        //print(health);
        if (health != null) {
            health.DealDamage(damage);
        }

        Instantiate(hitPrefab, pos, rotation);
    }
}
