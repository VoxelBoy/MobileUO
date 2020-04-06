using System;
using UnityEngine;
using UnityEngine.UI;

public class ScissorButtonPresenter : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text text;

    private int scissorStateOverrideIndex;
    private Array scissorStateOverrideValues;
    private int scissorStateOverrideValuesLength;

    void Awake()
    {
        scissorStateOverrideValues = System.Enum.GetValues(typeof(ScissorStateOverride));
        scissorStateOverrideValuesLength = scissorStateOverrideValues.Length;
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        scissorStateOverrideIndex++;
        if (scissorStateOverrideIndex >= scissorStateOverrideValuesLength)
        {
            scissorStateOverrideIndex -= scissorStateOverrideValuesLength;
        }
        var scissorStateOverride = (ScissorStateOverride) scissorStateOverrideValues.GetValue(scissorStateOverrideIndex);
        FindObjectOfType<UnityMain>().ScissorStateOverride = scissorStateOverride;
        text.text = $"Scissor: {scissorStateOverride.ToString()}";
    }
}