using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainUIElement : UIScript
{
    [SerializeField] private Button shootButton;
    public override void Initialize()
    {
        base.Initialize();
        shootButton.onClick.AddListener(() =>
        {
            InputHolder.Instance.button_0 = true;
        });
    }
    public override void Show()
    {
        base.Show();
    }
    public override void Hide()
    {
        base.Hide();
    }
}
