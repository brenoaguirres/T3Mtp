using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject _crossArrowIcon;
    [SerializeField] private GameObject _crossYouText;
    [SerializeField] private GameObject _circleArrowIcon;
    [SerializeField] private GameObject _circleYouText;

    private void Awake()
    {
        _crossArrowIcon.SetActive(false);
        _crossYouText.SetActive(false);
        _circleArrowIcon.SetActive(false);
        _circleYouText.SetActive(false);
    }

    private void Start()
    {
        GameManager.Singleton.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Singleton.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        if (GameManager.Singleton.LocalPlayerType == GameManager.PlayerType.Cross)
        {
            _crossYouText.SetActive(true);
        }
        else
        {
            _circleYouText.SetActive(true);
        }

        UpdateCurrentArrow();
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Singleton.CurrentPlayablePlayerType.Value == GameManager.PlayerType.Cross)
        {
            _crossArrowIcon.SetActive(true);
            _circleArrowIcon.SetActive(false);
        }
        else
        {
            _circleArrowIcon.SetActive(true);
            _crossArrowIcon.SetActive(false);
        }
    }
}
