using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarController : MonoBehaviour {
	public static StarController s;

	private void Awake() {
		s = this;
	}

	
	public int cargoStars = 2;
	public int speedStars = 2;


	public Image[] speedStarsImages;
	public Image[] cargoStarsImages;
	
	public Color starActiveColor = Color.white;
	public Color starLostColor = Color.grey;

	public void UpdateSpeedStars(int count) {
		if (count != speedStars) {
			speedStars = count;

			SetStarAmount(speedStarsImages, speedStars);
		}
	}
	
	public void UpdateCargoStars(int count) {
		if (count != cargoStars) {
			cargoStars = count;

			SetStarAmount(cargoStarsImages, cargoStars);
		}
	}

	void SetStarAmount(Image[] stars, int amount) {
		if(stars[0] == null || stars[1] == null)
			return;
		
		switch (amount) {
			case 2:
				stars[0].color = starActiveColor;
				stars[1].color = starActiveColor;
				break;
			case 1:
				stars[0].color = starActiveColor;
				stars[1].color = starLostColor;
				break;
			case 0:
				stars[0].color = starLostColor;
				stars[1].color = starLostColor;
				break;
		}
	}
}
