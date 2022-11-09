using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI inst;
    [SerializeField] private Text _text_Score;
    [SerializeField] private Text _text_MoveLimit;
    [SerializeField] private Text _text_ToyLimit;
    [SerializeField] private GameObject _popup_Clear;
    [SerializeField] private GameObject _popup_Fail;

    private void Awake()
    {
        if (!inst) inst = this;
    }

    private void Update()
    {
        // TODO - StageMng 요소들 옵저버 패턴 로직으로 변경
        if (StageMng.inst == null) return;
        _text_Score.text = StageMng.inst.Score.ToString();
        _text_MoveLimit.text = StageMng.inst.MoveLimit.ToString();
        _text_ToyLimit.text = StageMng.inst.ToyTopLimit.ToString();
    }

    public void Btn_Replay()
    {
        StageMng.inst.RePlay();
    }

    public void ShowResultPopup(bool isClear)
    {
        GameObject popup = isClear ? _popup_Clear : _popup_Fail; 
        popup.SetActive(true);
    }
}
