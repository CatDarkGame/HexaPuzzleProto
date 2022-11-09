using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageMng : MonoBehaviour
{
    public static StageMng inst;

    private int _moveLimit = 20;
    private int _toyTopLimit = 15;
    private int _score = 0;
    public int MoveLimit { get { return _moveLimit; } }
    public int ToyTopLimit { get { return _toyTopLimit; } }
    public int Score { get { return _score; } }


    [SerializeField] private TileController _tileController;

    private void Awake()
    {
        if (!inst) inst = this;
    }

    private void Start()
    {
        Init_Stage();

    }

    private void Init_Stage()
    {
        _moveLimit = 20;
        _toyTopLimit = 15;
        _score = 0;
        GridMap.inst.GenerateGrid();
        _tileController.ReloadGridMap();
    }

    public int CheckClearGame()
    {
        if (_toyTopLimit <= 0) return 1;
        if (_moveLimit <= 0) return 2;
        return 0;
    }

    public void FinishGame(bool isClear)
    {
        GameUI.inst.ShowResultPopup(isClear);
        StartCoroutine(Cor_Result());
    }

    IEnumerator Cor_Result()
    {
        yield return new WaitForSeconds(1.0f);
        while(true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RePlay();
                break;
            }
            yield return null;
        }
    }

    public void ScoreUp(int score)
    {
        _score += score;
    }

    public void ToyTopDown()
    {
        _toyTopLimit--;
        if (_toyTopLimit < 0) _toyTopLimit = 0;
    }

    public void MoveDown()
    {
        _moveLimit--;
        if (_moveLimit < 0) _moveLimit = 0;
    }

    public void RePlay()
    {
        SceneManager.LoadScene(0);
    }
}
