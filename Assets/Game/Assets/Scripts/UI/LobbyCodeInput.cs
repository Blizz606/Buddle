
using System.Collections;
using TMPro;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class LobbyCodeInput : MonoBehaviour
{
    [Header("Code Input Fields")]
    [SerializeField] private TMP_InputField[] fields;

    private Coroutine focusRoutine;
    private int lastNavigationFrame = -1;

    private void Start()
    {
        for (int i = 0; i < fields.Length; i++)
        {
            int index = i;

            fields[i].characterLimit = 1;

            fields[i].onValidateInput = (text, position, character) =>
                char.IsDigit(character) && character <= '9'
                    ? character
                    : '\0';

            fields[i].onValueChanged.AddListener(value =>
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (index < fields.Length - 1)
                        FocusField(index + 1);
                    else
                        Debug.Log("Lobby Code: " + GetCode());
                }
                else if (index > 0)
                {
                    FocusField(index - 1);
                }
            });
        }
    }

    private void Update()
    {
        bool backspace = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            backspace = Keyboard.current.backspaceKey.wasPressedThisFrame;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        backspace |= Input.GetKeyDown(KeyCode.Backspace);
#endif

        if (!backspace || lastNavigationFrame == Time.frameCount)
            return;

        for (int i = 1; i < fields.Length; i++)
        {
            if (fields[i].isFocused && string.IsNullOrEmpty(fields[i].text))
            {
                FocusField(i - 1);
                break;
            }
        }
    }

    private void FocusField(int index)
    {
        if (index < 0 || index >= fields.Length)
            return;

        lastNavigationFrame = Time.frameCount;

        if (focusRoutine != null)
            StopCoroutine(focusRoutine);

        focusRoutine = StartCoroutine(ApplyFocus(index));
    }

    private IEnumerator ApplyFocus(int index)
    {
        yield return null;

        fields[index].Select();
        fields[index].ActivateInputField();

        focusRoutine = null;
    }

    public string GetCode()
    {
        string code = "";

        foreach (TMP_InputField field in fields)
            code += field.text;

        return code;
    }
}
