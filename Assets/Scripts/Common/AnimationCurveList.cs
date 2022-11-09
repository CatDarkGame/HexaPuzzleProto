using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ִϸ��̼� Ŀ�� ������ ��ũ��Ʈ
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
