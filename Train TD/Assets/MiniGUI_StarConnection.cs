using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGUI_StarConnection : MonoBehaviour {
	private LineRenderer _lineRenderer;

    
	public Color backgroundColor = Color.blue;
	public float backgroundWidth = 0.12f;
	public Color previousPathColor = Color.blue;
	public float previousPathWidth = 0.2f;
	public Color highlightColor = Color.cyan;
	public float highlightWidth = 0.26f;

	public MiniGUI_Star source;
	public MiniGUI_Star target;
	public void Initialize(MiniGUI_Star _from, MiniGUI_Star _to) {
		source = _from;
		target = _to;
		_lineRenderer = GetComponent<LineRenderer>();
		SetHighlightState(false);
	}

	private void Start() {
		_lineRenderer = GetComponent<LineRenderer>();
	}

	private void Update() {
		_lineRenderer.SetPosition(0, transform.InverseTransformPoint(source.transform.position));
		_lineRenderer.SetPosition(1, transform.InverseTransformPoint(target.transform.position));
	}

	public void SetHighlightState(bool isHighlight) {
		if (isHighlight) {
			_lineRenderer.material.SetColor("mainColor", highlightColor);
			_lineRenderer.widthMultiplier = highlightWidth;
		} else {
			_lineRenderer.material.SetColor("mainColor", backgroundColor);
			_lineRenderer.widthMultiplier = backgroundWidth;
		}
	}

	public void SetPreviouslyVisited() {
		_lineRenderer.material.SetColor("mainColor", previousPathColor);
		_lineRenderer.widthMultiplier = previousPathWidth;
	}
}