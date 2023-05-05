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
            case ResourceTypes.scraps:
                scrapLocation = transform;
                break;
            default:
                // do nothing
                break;
        }
    }


    public static Transform GetSpawnLocation(ResourceTypes types) {
        switch (types) {
            case ResourceTypes.scraps:
                return scrapLocation;
            default:
                return null;
        }
    }
}
