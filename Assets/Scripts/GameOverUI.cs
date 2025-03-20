using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultTextMesh;
    [SerializeField] private Color _winColor;
    [SerializeField] private Color _loseColor;
    [SerializeField] private Button _rematchButton;

    private void Awake()
    {
        
        _rematchButton.onClick.AddListener(() =>
        {
            GameManager.Singleton.RematchRpc();
        });
    }

    private void Start()
    {
        GameManager.Singleton.OnGameWin += GameManager_OnGameWin;
        GameManager.Singleton.OnRematch += GameManager_OnRematch;
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.Singleton.LocalPlayerType)
        {
            _resultTextMesh.text = "YOU WIN";
            _resultTextMesh.color = _winColor;
        }
        else
        {
            _resultTextMesh.text = "YOU LOSE";
            _resultTextMesh.color = _loseColor;
        }
        Show();
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        Hide();
    }
    
    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
