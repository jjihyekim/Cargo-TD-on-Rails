using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceParticleSpawnLocation : MonoBehaviour {
    
    public ResourceTypes myType;
    
    private static Transform scrapLocation;
    private static Transform ammoLocation;
    private static Transform fuelLocation;

    
    void Awake()
    {
        switch (myType) {
            case ResourceTypes.ammo:
                ammoLocation = transform;
                break;
            case ResourceTypes.fuel:
                fuelLocation = transform;
                break;
            case ResourceTypes.scraps:
                scrapLocation = transform;
                break;
            case ResourceTypes.money:
            default:
                // do nothing
                break;
        }
    }


    public static Transform GetSpawnLocation(ResourceTypes types) {
        switch (types) {
            case ResourceTypes.ammo:
                return ammoLocation;
            case ResourceTypes.fuel:
                return fuelLocation;
            case ResourceTypes.scraps:
                return scrapLocation;
            case ResourceTypes.money:
            default:
                return null;
        }
    }
}
