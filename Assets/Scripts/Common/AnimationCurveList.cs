using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 애니메이션 커브 데이터 스크립트
[CreateAssetMenu(fileName = "AnimationCurveList", menuName = "Scriptable Object/AnimationCurveList", order = int.MaxValue)]
public class AnimationCurveList : ScriptableObject
{
    [System.Serializable]
    public struct GraphStruct
    {
        public string name;
        [SerializeField] public AnimationCurve curve;
    }

    [SerializeField] private GraphStruct[] _animationGraphs;

    public AnimationCurve GetAnimationGraph(string name)
    {
        if (_animationGraphs==null || 
            string.IsNullOrEmpty(name)) return null;

        for(int i=0;i<_animationGraphs.Length;i++)
        {
            GraphStruct graph = _animationGraphs[i];
            if (graph.name.Equals(name)) return graph.curve;
        }
        return null;
    }
}
