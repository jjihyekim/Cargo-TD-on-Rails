using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapPileMaker : MonoBehaviour {
    public GameObject pilePart;
    public int count = 4;

    public float spreadMultiplier = 0.25f;
    public Vector2 sizeRange = new Vector2(0.1f, 0.2f);
    
    public void MakePile(int count, bool clear = true) {
        if(clear)
            Clear();

        this.count = count;
        
        for (int i = 0; i < count; i++) {
            var pile = Instantiate(pilePart, transform);
            pile.transform.localPosition = Random.insideUnitSphere * Mathf.Pow(count, 1f / 3f) * spreadMultiplier;
            pile.transform.rotation = Random.rotation;
            pile.transform.localScale = Vector3.one * Random.Range(sizeRange.x, sizeRange.y);
        }
    }
    
    public void EditorMakePile() {
        EditorClear();
        MakePile(4, false);
    }

    void Clear() {
        var childCount = transform.childCount;
        for (int i = childCount-1; i >= 0; i--) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    void EditorClear() {
        var childCount = transform.childCount;
        for (int i = childCount-1; i >= 0; i--) {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
    
    
#if UNITY_EDITOR
    [MethodButton("EditorMakePile", "EditorClear")]
    [SerializeField] private bool editorFoldout;
#endif
}
