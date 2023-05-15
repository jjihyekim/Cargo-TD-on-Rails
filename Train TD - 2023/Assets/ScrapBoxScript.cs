using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScrapBoxScript : MonoBehaviour
{

    public Transform dropPointAndParent;
    public GameObject scrapPrefab;
    
    public List<ScrapPieceScript> myScraps = new List<ScrapPieceScript>();

    public float maxScrap = 100;
    public float curScrap;
    public float targetScraps;

    public int maxPieces = 50;
    public float scrapPerPiece = 5;
    [Tooltip("Pieces smaller than this won't be added")]
    public float mininumChunkPercent = 0.2f;

    public TMP_Text uiText;

    public float maxSize = 0.05f;
    public float minSize = 0.01f;
    //public float minSizeCapacity = 50;
    //public float maxSizeCapacity = 50;
    
    public float scaler = 1f;
    public void SetMaxScrap(float amount) {
	    maxScrap = amount;
	    scaler = maxScrap;
	    scaler = Mathf.Clamp(scaler, 50, 200);
	    scaler = scaler.Remap(50, 200, 1f, 0.7f);
	    //Debug.Log(scaler);
	    scrapPerPiece = maxScrap / maxPieces;
	    scrapPerPiece = Mathf.Clamp(scrapPerPiece, 1, float.MaxValue);
    }
    

    public void SetScrap(float amount) {
	    uiText.text = $"{amount:F0}/{maxScrap:F0}";
	    if(Mathf.Approximately( curScrap, amount))
		    return;

	    targetScraps = amount;
    }

    public float randomDropMagnitude = 0.3f;

    public float timer = 0;
    private void Update() {
	    var delta = targetScraps - curScrap;

	    if (Mathf.Abs(delta) >= scrapPerPiece*mininumChunkPercent) {
		    if (delta > 0) {
			    if (timer <= 0) {
				    timer = 0.05f;
				    AddScrap(delta);
			    } else {
				    timer -= Time.deltaTime;
			    }
		    } else if (delta < 0) {
			    if (timer <= 0) {
				    timer = 0.05f;
				    RemoveScrap(-delta);
			    } else {
				    timer -= Time.deltaTime;
			    }
		    }
	    }
    }

    void AddScrap(float amount) {
		if (scrapPerPiece > 0.1f) {
			var scrap = Instantiate(scrapPrefab, dropPointAndParent, false);
			scrap.transform.position += Vector3.left * Random.Range(-randomDropMagnitude, randomDropMagnitude);
			var scrapScript = scrap.GetComponent<ScrapPieceScript>();
			var targetAmount = scrapPerPiece;
			if (amount < scrapPerPiece)
				targetAmount = amount;
			
			if(targetAmount < scrapPerPiece*mininumChunkPercent)
				return;
			
			scrapScript.SetUpScrapPiece(targetAmount ,scrapPerPiece, maxSize * scaler, minSize * scaler);
			myScraps.Add(scrapScript);
			curScrap += targetAmount;
		} else {
			Debug.LogError("Scrap per piece is too smal");
		}
	}

	void RemoveScrap(float amount) {
		foreach (var scrap in myScraps.OrderBy(x => x.myAmount)) {
			var realAmount = scrap.myAmount;
			if (amount >= realAmount) {
				scrap.DestroySelf();
				myScraps.Remove(scrap);
				curScrap -= realAmount;
			} else {
				scrap.SetUpScrapPiece((realAmount-amount),scrapPerPiece,maxSize * scaler,minSize * scaler);
				//print(realAmount - amount);
				curScrap -= amount;
				break;
			}
			
			return;
		}
	}


	public float debugAmount = 5f;
	[Button]
	public void AddScrapDebug() {
		SetScrap(curScrap + debugAmount);
	}
	
	[Button]
	public void RemoveScrapDebug() {
		SetScrap(curScrap - debugAmount);
	}
}
