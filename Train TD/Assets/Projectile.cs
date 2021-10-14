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

    public bool isPlayerBullet = false;

    public Transform target {
        get {
            return source.target;
        }
    }
    public GunModule source;

    
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
        if (!isDead) {
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
                if (Vector3.Distance(transform.position, target.position) < (curSpeed+0.1f) * Time.deltaTime) {
                    DestroyFlying();
                }
            }

            GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * curSpeed * Time.deltaTime);
        }
    }

    private bool isDead = false;

    public GameObject instantDestroy;

    void SmartDestroySelf() {
        if (!isDead) {
            isDead = true;

            var particles = GetComponentsInChildren<ParticleSystem>();

            foreach (var particle in particles) {
                if (particle.gameObject != instantDestroy) {
                    particle.transform.SetParent(null);
                    particle.Stop();
                    Destroy(particle.gameObject, 5f);
                }
            }
            
            Destroy(instantDestroy);
            Destroy(gameObject);
        }
    }

    private void DestroyFlying() {
        if (!isDead) {
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
    }


    private void OnCollisionEnter(Collision other) {
        if (!isDead) {
            if (other.transform.root.gameObject != myOriginObject) {
                var otherProjectile = other.transform.root.GetComponent<Projectile>();
                if (otherProjectile != null) {
                    if (otherProjectile.isPlayerBullet == isPlayerBullet) {
                        // we don't want projectiles from the same faction collide with each other
                        return;
                    }
                }

                var train = other.transform.root.GetComponent<Train>();

                if (train != null && isPlayerBullet) {
                    // make player bullets dont hit the player
                    return;
                }

                var enemy = other.transform.root.GetComponent<EnemyTypeData>();
                
                if (enemy != null && !isPlayerBullet) {
                    // make enemy projectiles not hit the player
                    return;
                }
                
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
