using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHolder : MonoBehaviour
{
    private static InputHolder _instance;
    public static InputHolder Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InputHolder>();
            }
            return _instance;
        }
    }
    public bool button_0;

    private IEnumerator Start()
    {
        while (true)
        {
            yield return Helpers.GetWait(0.1f);
            ResetInput();
        }
    }

    public void PressButton0()
    {
        button_0 = true;
    }

    private void ResetInput()
    {
        button_0 = false;
    }
}
