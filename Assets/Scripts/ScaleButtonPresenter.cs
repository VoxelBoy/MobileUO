using System;
using UnityEngine;
using UnityEngine.UI;

public class ScaleButtonPresenter : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text text;

    private int scaleSizeIndex;
    private Array scaleSizesValues;
    private int scaleSizesValuesLength;

    void Awake()
    {
        scaleSizesValues = System.Enum.GetValues(typeof(ScaleSizes));
        scaleSizesValuesLength = scaleSizesValues.Length;
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        scaleSizeIndex++;
        if (scaleSizeIndex >= scaleSizesValuesLength)
        {
            scaleSizeIndex -= scaleSizesValuesLength;
        }
        var scaleSize = (ScaleSizes) scaleSizesValues.GetValue(scaleSizeIndex);
        FindObjectOfType<UnityMain>().CustomScaleSize = scaleSize;
        text.text = $"Scale: {((int) scaleSize)}%";
    }
}
