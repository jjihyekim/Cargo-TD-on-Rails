using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Projectile : MonoBehaviour {
    private float curSpeed = 0;
    private float curSeekStrength = 0;
    public float acceleration = 5f;
    public float seekAcceleration = 200f;
    public float speed = 5f;
    public float damage = 20f;
    public float mortarRange = 2f;

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
        Bullet, Rocket, Mortar
    }

    public HitType myHitType = HitType.Bullet;

    public Vector3 mortarGravity = new Vector3(0, -9.81f, 0f);
    private Vector3 mortarVelocity;
    private void Start() {
        Invoke("DestroySelf", lifetime);

        switch (myHitType) {
            case HitType.Bullet:
                curSpeed = speed;
                curSeekStrength = seekStrength;
                break;
            
            case HitType.Rocket:
                curSpeed = 0;
                curSeekStrength = 0;
                break;
            
            case HitType.Mortar:
                curSpeed = speed;
                curSeekStrength = seekStrength;
                
                float angle = 90- Vector3.Angle(Vector3.up, transform.forward);
                float gx = mortarGravity.y * Vector3.Distance(transform.position, target.position);
                float sinVal = 0.5f* Mathf.Sin(angle);
                float velocity = Mathf.Sqrt(Mathf.Abs(gx / sinVal));

                mortarVelocity = transform.forward * velocity;
                break;
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

            switch (myHitType) {
                case HitType.Bullet:
                case HitType.Rocket:
                    
                    GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * curSpeed * Time.deltaTime);
                    break;
                case HitType.Mortar:
                    GetComponent<Rigidbody>().MovePosition(transform.position + (transform.forward * curSpeed + mortarVelocity) * Time.deltaTime);
                    mortarVelocity += mortarGravity * Time.deltaTime;
                    break;
            }
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
                case HitType.Mortar:
                    hitPrefab = null;
                    break;
            }

            if (myHitType != HitType.Mortar) {
                var health = target.GetComponentInParent<IHealth>();

                if (health != null) {
                    health.DealDamage(damage);
                }
            } else {
                MortarDamage();
            }

            if (hitPrefab != null) {
                Instantiate(hitPrefab, transform.position, transform.rotation);
            }

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
                    // make enemy projectiles not hit the player projectiles
                    return;
                }
                
                switch (myHitType) {
                    case HitType.Bullet:
                        ContactDamage(other);
                        break;
                    case HitType.Rocket:
                        ExplosiveDamage(other);
                        break;
                    case HitType.Mortar:
                        MortarDamage();
                        break;
                }

                SmartDestroySelf();
            }
        }
    }
    
    private void MortarDamage() {
        GameObject hitPrefab = LevelReferences.s.mortarExplosionEffectPrefab;
        GameObject miniHitPrefab = LevelReferences.s.mortarMiniHitPrefab;

        var targets = Physics.OverlapSphere(transform.position, mortarRange);

        var healthsInRange = new List<IHealth>();
        var healthsInRangeGms = new List<GameObject>();
        for (int i = 0; i < targets.Length; i++) {
            var target = targets[i];
            
            var health = target.gameObject.GetComponentInParent<IHealth>();
            if (health != null && !health.IsPlayer()) {
                if (!healthsInRange.Contains(health)) {
                    healthsInRange.Add(health);
                }
            }


            
        }

        foreach (var health in healthsInRange) {
            health.DealDamage(damage);
            Instantiate(miniHitPrefab, health.GetGameObject().transform.position, Quaternion.identity);
        }
        Instantiate(hitPrefab, transform.position, Quaternion.identity);
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
