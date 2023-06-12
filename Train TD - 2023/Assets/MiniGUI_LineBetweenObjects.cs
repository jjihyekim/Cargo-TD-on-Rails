using UnityEngine;
using UnityEngine.UI;
 
public class MiniGUI_LineBetweenObjects : MonoBehaviour
{
	private RectTransform object1;
	private RectTransform object2;
	private RawImage image;
	private RectTransform rectTransform;

	public float height = 5;
	public float gap = 5;

	public float scrollSpeed = 1f;
	// Start is called before the first frame update
	void Start()
	{
		image = GetComponent<RawImage>();
		rectTransform = GetComponent<RectTransform>();
	}
 
	public void SetObjects(GameObject one, GameObject two)
	{
		object1 = one.GetComponent<RectTransform>();
		object2 = two.GetComponent<RectTransform>();
 
		RectTransform aux;
		if (object1.localPosition.x > object2.localPosition.x)
		{
			aux = object1;
			object1 = object2;
			object2 = aux;
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (object1.gameObject.activeSelf && object2.gameObject.activeSelf)
		{
			rectTransform.localPosition = (object1.localPosition + object2.localPosition) / 2;
			Vector3 dif = object2.localPosition - object1.localPosition;
			var realDiff = dif.magnitude - gap * 2;
			rectTransform.sizeDelta = new Vector3(realDiff, height);
			rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));
			var uvRect = image.uvRect;
			uvRect.width = realDiff / 10;
			uvRect.x += Time.deltaTime * scrollSpeed;
			image.uvRect = uvRect;
		}
	}
}