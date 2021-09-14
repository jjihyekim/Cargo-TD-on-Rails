using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunModule : MonoBehaviour, IComponentWithTarget {
    public TransformWithActivation[] rotateTransforms;
    public TransformWithActivation[] barrelEndTransforms;
    public float projectileSpawnOffset = 0.2f;
    

    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;


    public Transform target;


    public float fireDelay = 2f;
    public int fireBarrageCount = 5;
    public float fireBarrageDelay = 0.1f;
    public float projectileDamage = 2f;

    public float rotateSpeed = 10f;

    public bool CanShoot = false;
    private void Update() {
        if (target != null) {
            for (int i = 0; i < rotateTransforms.Length; i++) {
                var rotateTransform = rotateTransforms[i].transform;
                var lookRotation = Quaternion.LookRotation(target.position - rotateTransform.position, Vector3.up);
                rotateTransform.rotation = Quaternion.Lerp(rotateTransform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
                if (Quaternion.Angle(rotateTransform.rotation, lookRotation) < 5) {
                    CanShoot = true;
                }
            }

            if (rotateTransforms.Length == 0) {
                CanShoot = true;
            }
        }
    }


    private IEnumerator ActiveShootCycle;
    private bool isShooting = false;
    IEnumerator ShootCycle() {
        while (true) {
            yield return new WaitForSeconds(fireDelay);
            StartCoroutine(ShootBarrage());
        }
    }

    IEnumerator ShootBarrage() {
        for (int i = 0; i < fireBarrageCount; i++) {
            while (!CanShoot) {
                yield return null;
            }

            var barrelEnd = GetShootTransform().transform;
            var position = barrelEnd.position;
            var rotation = barrelEnd.rotation;
            var bullet = Instantiate(bulletPrefab, position + barrelEnd.forward * projectileSpawnOffset, rotation);
            var muzzleFlash = Instantiate(muzzleFlashPrefab, position, rotation);
            var projectile = bullet.GetComponent<Projectile>();
            projectile.myOriginObject = this.gameObject;
            projectile.damage = projectileDamage;
            projectile.isTargetSeeking = true;
            
            
            projectile.target = target;
            

            yield return new WaitForSeconds(fireBarrageDelay);
        }
    }

    private int lastIndex = -1;
    public TransformWithActivation GetShootTransform() {
        List<TransformWithActivation> activeTransforms = new List<TransformWithActivation>();

        for (int i = 0; i < barrelEndTransforms.Length; i++) {
            if (barrelEndTransforms[i].transform.gameObject.activeInHierarchy) {
                activeTransforms.Add(barrelEndTransforms[i]);
            }
        }

        if (activeTransforms.Count == 0) {
            activeTransforms.Add(barrelEndTransforms[0]);
        }

        lastIndex++;
        lastIndex = lastIndex % activeTransforms.Count;
        return activeTransforms[lastIndex];
    }

    public void SetTarget(Transform target) {
        this.target = target;
        if (!isShooting) {
            ActiveShootCycle = ShootCycle();
            StartCoroutine(ActiveShootCycle);
            isShooting = true;
        }
    }

    public void UnsetTarget() {
        this.target = null;
        if (isShooting) {
            StopCoroutine(ActiveShootCycle);
            isShooting = false;
        }
    }

    public Transform GetRangeOrigin() {
        return transform; 
    }

    public Transform GetActiveTarget() {
        return target;
    }
}


[System.Serializable]
public class TransformWithActivation {
    public Transform transform;
}