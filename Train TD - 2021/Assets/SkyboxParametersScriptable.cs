using System.Collections;
using System.Collections.Generic;
using Borodar.FarlandSkies.LowPoly;
using Borodar.FarlandSkies.LowPoly.DotParams;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu]
public class SkyboxParametersScriptable : ScriptableObject {
	public Material SkyboxMaterial;

	// Sky

	[SerializeField] [Tooltip("Color at the top pole of skybox sphere")]
	private Color _topColor = Color.gray;

	[SerializeField] [Tooltip("Color on equator of skybox sphere")]
	private Color _middleColor = Color.gray;

	[SerializeField] [Tooltip("Color at the bottom pole of skybox sphere")]
	private Color _bottomColor = Color.gray;

	[SerializeField] [Range(0.01f, 5f)] [Tooltip("Color interpolation coefficient between top pole and equator")]
	private float _topExponent = 1f;

	[SerializeField] [Range(0.01f, 5f)] [Tooltip("Color interpolation coefficient between bottom pole and equator")]
	private float _bottomExponent = 1f;

	// Stars

	[SerializeField] private bool _starsEnabled = true;

	[SerializeField] private Cubemap _starsCubemap;

	[SerializeField] private Color _starsTint = Color.gray;

	[SerializeField] [Range(0f, 10f)] [Tooltip("Reduction in stars apparent brightness closer to the horizon")]
	private float _starsExtinction = 2f;

	[SerializeField] [Range(0f, 25f)] [Tooltip("Variation in stars apparent brightness caused by the atmospheric turbulence")]
	private float _starsTwinklingSpeed = 10f;

	// Sun

	[SerializeField] private bool _sunEnabled = true;

	[SerializeField] private Texture2D _sunTexture;

	private Light _sunLight;

	[SerializeField] private Color _sunTint = Color.gray;

	[SerializeField] [Range(0.1f, 3f)] private float _sunSize = 1f;

	[SerializeField] [Range(0f, 2f)] private float _sunHalo = 1f;

	[SerializeField] private bool _sunFlare = true;

	[SerializeField] [Range(0.01f, 2f)] [Tooltip("Actual flare brightness depends on sun tint alpha, and this property is just a coefficient for that value")]
	private float _sunFlareBrightness = 0.3f;

	// Moon

	[SerializeField] private bool _moonEnabled = true;

	[SerializeField] private Texture2D _moonTexture;

	 private Light _moonLight;

	[SerializeField] private Color _moonTint = Color.gray;

	[SerializeField] [Range(0.1f, 3f)] private float _moonSize = 1f;

	[SerializeField] [Range(0f, 2f)] private float _moonHalo = 1f;

	[SerializeField] private bool _moonFlare = true;

	[SerializeField] [Range(0.01f, 2f)] [Tooltip("Actual flare brightness depends on moon tint alpha, and this property is just a coefficient for that value")]
	private float _moonFlareBrightness = 0.3f;

	// Clouds

	[SerializeField] private bool _cloudsEnabled = true;

	[SerializeField] private Cubemap _cloudsCubemap;

	[SerializeField] private Color _cloudsTint = Color.gray;

	[SerializeField] [Range(-0.75f, 0.75f)] [Tooltip("Height of the clouds relative to the horizon.")]
	private float _cloudsHeight;

	[SerializeField] [Range(0, 360f)] [Tooltip("Rotation of the clouds around the positive y axis.")]
	private float _cloudsRotation;

	// General

	[SerializeField] [Range(0, 8f)] private float _exposure = 1f;

	[SerializeField] [Tooltip("Keep fog color in sync with the sky middle color automatically")]
	private bool _adjustFogColor;

	// Private

	private LensFlare _sunFlareComponent;
	private LensFlare _moonFlareComponent;

	[Button]
	public void GetActiveSkybox(SkyboxController instance) {
		var target = instance;
		Debug.Log($"Getting {target.gameObject.name} settings");

		SkyboxMaterial = target.SkyboxMaterial;
		_topColor = target.TopColor;
		_middleColor = target.MiddleColor;
		_bottomColor = target.BottomColor;
		_topExponent = target.TopExponent;
		_bottomExponent = target.BottomExponent;

		_starsEnabled = target.StarsEnabled;
		_starsCubemap = target.StarsCubemap;
		_starsTint = target.StarsTint;
		_starsExtinction = target.StarsExtinction;
		_starsTwinklingSpeed = target.StarsTwinklingSpeed;

		_sunEnabled = target.SunEnabled;
		_sunTexture = target.SunTexture;
		//_sunLight = target.SunLight;
		_sunTint = target.SunTint;
		_sunSize = target.SunSize;
		_sunHalo = target.SunHalo;
		_sunFlare = target.SunFlare;
		_sunFlareBrightness = target.SunFlareBrightness;

		_moonEnabled = target.MoonEnabled;
		_moonTexture = target.MoonTexture;
		//_moonLight = target.MoonLight;
		_moonTint = target.MoonTint;
		_moonSize = target.MoonSize;
		_moonHalo = target.MoonHalo;
		_moonFlare = target.MoonFlare;
		_moonFlareBrightness = target.MoonFlareBrightness;

		_cloudsEnabled = target.CloudsEnabled;
		_cloudsCubemap = target.CloudsCubemap;
		_cloudsTint = target.CloudsTint;
		_cloudsHeight = target.CloudsHeight;
		_cloudsRotation = target.CloudsRotation;

		_exposure = target.Exposure;
		_adjustFogColor = target.AdjustFogColor;
	}

	[Button]
	public void GetActiveSkybox() {
		var target = SkyboxController.Instance;
		Debug.Log($"Getting {target.gameObject.name} settings");

		SkyboxMaterial = target.SkyboxMaterial;
		_topColor = target.TopColor;
		_middleColor = target.MiddleColor;
		_bottomColor = target.BottomColor;
		_topExponent = target.TopExponent;
		_bottomExponent = target.BottomExponent;

		_starsEnabled = target.StarsEnabled;
		_starsCubemap = target.StarsCubemap;
		_starsTint = target.StarsTint;
		_starsExtinction = target.StarsExtinction;
		_starsTwinklingSpeed = target.StarsTwinklingSpeed;

		_sunEnabled = target.SunEnabled;
		_sunTexture = target.SunTexture;
		//_sunLight = target.SunLight;
		_sunTint = target.SunTint;
		_sunSize = target.SunSize;
		_sunHalo = target.SunHalo;
		_sunFlare = target.SunFlare;
		_sunFlareBrightness = target.SunFlareBrightness;

		_moonEnabled = target.MoonEnabled;
		_moonTexture = target.MoonTexture;
		//_moonLight = target.MoonLight;
		_moonTint = target.MoonTint;
		_moonSize = target.MoonSize;
		_moonHalo = target.MoonHalo;
		_moonFlare = target.MoonFlare;
		_moonFlareBrightness = target.MoonFlareBrightness;

		_cloudsEnabled = target.CloudsEnabled;
		_cloudsCubemap = target.CloudsCubemap;
		_cloudsTint = target.CloudsTint;
		_cloudsHeight = target.CloudsHeight;
		_cloudsRotation = target.CloudsRotation;

		_exposure = target.Exposure;
		_adjustFogColor = target.AdjustFogColor;
	}

	[Button]
	public void SetActiveSkybox(Light sunlight, Light moonlight) {
		_sunLight = sunlight;
		_moonLight = moonlight;
		
		
		var target = SkyboxController.Instance;
		//Debug.Log($"Setting {target.gameObject.name} settings");

		target.SkyboxMaterial = SkyboxMaterial;
		target.TopColor = _topColor;
		target.MiddleColor = _middleColor;
		target.BottomColor = _bottomColor;
		target.TopExponent = _topExponent;
		target.BottomExponent = _bottomExponent;

		target.StarsEnabled = _starsEnabled;
		target.StarsCubemap = _starsCubemap;
		target.StarsTint = _starsTint;
		target.StarsExtinction = _starsExtinction;
		target.StarsTwinklingSpeed = _starsTwinklingSpeed;

		target.SunEnabled = _sunEnabled;
		target.SunTexture = _sunTexture;
		target.SunLight = _sunLight;
		target.SunTint = _sunTint;
		target.SunSize = _sunSize;
		target.SunHalo = _sunHalo;
		target.SunFlare = _sunFlare;
		target.SunFlareBrightness = _sunFlareBrightness;

		target.MoonEnabled = _moonEnabled;
		target.MoonTexture = _moonTexture;
		target.MoonLight = _moonLight;
		target.MoonTint = _moonTint;
		target.MoonSize = _moonSize;
		target.MoonHalo = _moonHalo;
		target.MoonFlare = _moonFlare;
		target.MoonFlareBrightness = _moonFlareBrightness;

		target.CloudsEnabled = _cloudsEnabled;
		target.CloudsCubemap = _cloudsCubemap;
		target.CloudsTint = _cloudsTint;
		target.CloudsHeight = _cloudsHeight;
		target.CloudsRotation = _cloudsRotation;

		target.Exposure = _exposure;
		target.AdjustFogColor = _adjustFogColor;

		target.UpdateSkyboxProperties();
	}
}
