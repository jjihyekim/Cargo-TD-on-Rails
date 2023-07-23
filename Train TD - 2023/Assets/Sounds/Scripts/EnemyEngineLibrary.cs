using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyEngineLibrary", menuName = "Audio/EnemyEngineLibrary")]
public class EnemyEngineLibrary : SerializedScriptableObject
{
    public Dictionary<EnemyType, EnemyEngineSfxData> sfxDict = new Dictionary<EnemyType, EnemyEngineSfxData>();
}

[HideReferenceObjectPicker]
public class EnemyEngineSfxData
{
    [LabelWidth(50)]
    public AudioClip clip;

    [LabelWidth(50)]
    public float volume;
}
