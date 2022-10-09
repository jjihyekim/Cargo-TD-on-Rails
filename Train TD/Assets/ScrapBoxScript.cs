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

    public TMP_Text uiText;

    private float maxSize = 0.05f;
    private float minSize = 0.01f;
    public float scaler = 1f;
    public void SetMaxScrap(float amount) {
	    maxScrap = amount;
	    scrapPerPiece = 5;

	    scaler = maxScrap;
	    scaler = scaler.Remap(50, 400, 1.6f, 0.65f);
	    Debug.Log(scaler);
    }


    public void SetScrap(float amount) {
	    if(Mathf.Approximately( curScrap, amount))
		    return;
	    targetScraps = amount;

	    uiText.text = $"{targetScraps:F0}/{maxScrap:F0}";
    }

    public float randomDropMagnitude = 0.3f;


    public float timer = 0;
    private void Update() {
	    if (targetScraps > curScrap) {
		    if (timer <= 0) {
			    timer = 0.1f;
			    AddScrap(targetScraps - curScrap);
		    } else {
			    timer -= Time.deltaTime;
		    }
	    }else if (targetScraps < curScrap) {
		    if (timer <= 0) {
			    timer = 0.1f;
			    RemoveScrap(curScrap - targetScraps);
		    } else {
			    timer -= Time.deltaTime;
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
			scrapScript.SetUpScrapPiece(targetAmount / scrapPerPiece, maxSize * scaler, minSize * scaler);
			myScraps.Add(scrapScript);
			curScrap += targetAmount;
		} else {
			Debug.LogError("Scrap per piece is too smal");
		}
	}

	void RemoveScrap(float amount) {
		foreach (var scrap in myScraps.OrderBy(x => x.myAmount)) {
			var realAmount = scrap.myAmount * scrapPerPiece;
			if (amount >= realAmount) {
				Destroy(scrap.gameObject);
				myScraps.Remove(scrap);
				curScrap -= realAmount;
			} else {
				scrap.SetUpScrapPiece((realAmount-amount)/scrapPerPiece,maxSize * scaler,minSize * scaler);
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
