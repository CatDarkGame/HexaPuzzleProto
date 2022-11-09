using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectMng : MonoBehaviour
{
    public static EffectMng inst;

    [System.Serializable]
    public struct EffectAsset
    {
        public string name;
        public GameObject prefab;
    }

    [SerializeField] private EffectAsset[] _effectAssets;

    private void Awake()
    {
        if (!inst) inst = this;
    }

    public GameObject CreateEffect(string assetName, Vector3 position)
    {
        int effectIndex = GetEffectAsset(assetName);
        if (effectIndex == -1) return null;
        EffectAsset asset = _effectAssets[effectIndex];
        if (asset.prefab == null) return null;

        GameObject go = Instantiate(asset.prefab);	// TODO : 오브젝트 폴링으로 대체
        if (go == null) return null;
        go.transform.SetParent(transform);
        go.transform.position = position;
        return go;
    }

    private int GetEffectAsset(string name)
    {
        if (_effectAssets == null ||
            string.IsNullOrEmpty(name)) return -1;
        for (int i = 0; i < _effectAssets.Length; i++)
        {
            EffectAsset asset = _effectAssets[i];
            if (asset.name.Equals(name)) return i;
        }
        return -1;
    }
}
