using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Field : MonoBehaviour
{
    public UIDocument UiDocument;
    public float Border = 0.1f;
    private Number[] Numbers;
    private Label WinLabel;

    private class ComparerNumbersByValue : IComparer<Number>
    {
        int IComparer<Number>.Compare(Number number1, Number number2)
        {
            return number1.Value.CompareTo(number2.Value);
        }
    }

    private class ComparerNumbersByPosition : IComparer<Number>
    {
        int IComparer<Number>.Compare(Number number1, Number number2)
        {
            return number1.Position.CompareTo(number2.Position);
        }
    }

    void Start()
    {
        Numbers = GenerateRandomNumberPositions();
        WinLabel = UiDocument.rootVisualElement.Q<Label>("WinLabel");
        WinLabel.style.display = DisplayStyle.None;
    }

    Number[] GenerateRandomNumberPositions()
    {
        Number[] numbers = GetSortedNumbers();
        var random = new System.Random();
        int n = numbers.Length;
        while (n > 1)
        {
            int k = random.Next(n--);
            (numbers[k], numbers[n]) = (numbers[n], numbers[k]);
        }

        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i].Position = i;
            numbers[i].GetComponent<Collider2D>().enabled = true;
        }
        return numbers;
    }

    Number[] GetSortedNumbers()
    {
        Number[] numbers = gameObject.GetComponentsInChildren<Number>();
        Array.Sort(numbers, new ComparerNumbersByValue());
        return numbers;
    }

    void Update()
    {
        if (IsWin())
        {
            Finish();
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.value), Vector2.zero);
            if (hit.collider)
            {
                PressNumber(hit.collider.gameObject.GetComponent<Number>());
            }
        }

        Render();
    }

    void PressNumber(Number number)
    {
        if (number.Value == 16)
            return;

        var hole = GetHole();
        if ((number.Position + 1 == hole.Position && (number.Position + 1) % 4 != 0) ||
            (number.Position - 1 == hole.Position && (number.Position + 1) % 4 != 1) ||
            (number.Position + 4 == hole.Position) ||
            (number.Position - 4 == hole.Position))
        {
            (number.Position, hole.Position) = (hole.Position, number.Position);
            Array.Sort(Numbers, new ComparerNumbersByPosition());
        }
    }

    Number GetNumber(int value)
    {
        var result = Array.Find<Number>(Numbers, number => number.Value == value);
        if (result == null)
            throw new IndexOutOfRangeException("Number not found: " + value);
        return result;
    }

    Number GetHole()
    {
        return GetNumber(16);
    }

    void Render()
    {
        Vector2 numberSize = Numbers[0].GetComponent<SpriteRenderer>().size;
        int x = 0;
        int y = 0;
        Vector3 startPosition = new(-1 * (numberSize.x * 1.5f + Border + Border / 2), numberSize.y * 1.5f + Border + Border / 2, 0f);
        foreach (Number number in Numbers)
        {
            number.transform.position = new Vector3(startPosition.x + (numberSize.x + Border) * x, startPosition.y - (numberSize.y + Border) * y, startPosition.z);
            ++x;
            if (x == 4)
            {
                x = 0;
                ++y;
            }
        }
    }

    bool IsWin()
    {
        foreach (Number number in Numbers)
        {
            if (number.Value == 14 || number.Value == 15)
                continue;

            if (number.Value != number.Position + 1)
            {
                return false;
            }
        }

        return true;
    }

    void Finish()
    {
        foreach (Number number in Numbers)
        {
            number.GetComponent<Collider2D>().enabled = false;
        }
        WinLabel.style.display = DisplayStyle.Flex;
    }

}
