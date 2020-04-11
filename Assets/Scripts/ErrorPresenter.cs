using System;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPresenter : MonoBehaviour
{
    [SerializeField]
    private Text errorText;

    [SerializeField]
    private Button backButton;

    public Action BackButtonClicked;

    private void OnEnable()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public void SetErrorText(string text)
    {
        errorText.text = text;
    }

    private void OnBackButtonClicked()
    {
        BackButtonClicked?.Invoke();
    }

    private void OnDisable()
    {
        backButton.onClick.RemoveAllListeners();
    }
}