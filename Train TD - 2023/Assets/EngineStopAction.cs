using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineStopAction : ModuleAction, IActiveDuringCombat
{
    [Space]
    public float actionTime = 25f;
    public int totalScrapsDropped = 50;
    public int scrapChunkAmount = 10;
    public float initialDelay = 2f;
    public Vector2 dropDistance = new Vector2(2, 5);
    private int originalPower;
    protected override void _EngageAction() {
        originalPower = GetComponent<EngineModule>().enginePower;

        GetComponent<EngineModule>().enginePower = 0;

        GetComponent<EngineOverloadAction>().canEngage = false;

        StartCoroutine(DropScraps());
    }

    IEnumerator DropScraps() {
        
        yield return new WaitForSeconds(initialDelay);

        var dropletCount = Mathf.FloorToInt((float)scrapChunkAmount / totalScrapsDropped);
        var delay = dropletCount * (actionTime-initialDelay);

        var totalDropped = 0;

        for (int i = 0; i < dropletCount; i++) {

            DropMoney(scrapChunkAmount);
            totalDropped += scrapChunkAmount;

            yield return new WaitForSeconds(delay);
        }

        if (totalDropped < totalScrapsDropped) {
            DropMoney(totalScrapsDropped-totalDropped);
        }
        
        StopAction();
    }

    void DropMoney(int amount) {
        var randomDirection = Random.onUnitSphere;
        randomDirection.y = 0;
        var randomDistance = Random.Range(dropDistance.x, dropDistance.y);
        randomDirection = randomDirection.normalized * randomDistance;
            
        LevelReferences.s.SpawnResourceAtLocation(ResourceTypes.scraps, amount, transform.position + randomDirection);
    }

    void StopAction() {
        GetComponent<EngineModule>().enginePower = originalPower;
        
        
        GetComponent<EngineOverloadAction>().canEngage = true;
    }
    
    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}
