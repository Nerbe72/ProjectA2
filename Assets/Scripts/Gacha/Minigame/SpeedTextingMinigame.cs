using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedTextingMinigame : Minigame
{
    public override MinigameType Type { get { return MinigameType.SpeedTexting; } }

    [SerializeField] private RectTransform parent;

    private GameObject textPrefab;
    private List<TMP_Text> targetText;

    private Queue<char> targetWord;
    private bool isGameEnd;
    private int textIndex;

    protected override void Init()
    {
        parent = GetComponentsInChildren<RectTransform>(true)[2];
        textPrefab = GetComponentInChildren<TMP_Text>(true).gameObject;
    }

    public override void SetGame()
    {
        int randomLength = Random.Range(5, 8);

        targetWord = new Queue<char>();
        targetText = new List<TMP_Text>();
        isGameEnd = false;
        textIndex = 0;

        for (int i = 0; i < randomLength; i++)
        {
            GameObject obj = Instantiate(textPrefab, parent.transform);
            TMP_Text text = obj.GetComponent<TMP_Text>();
            text.text = "";
            obj.SetActive(true);
            obj.transform.SetAsLastSibling();
            targetText.Add(text);
        }

        GenerateRandomWord(randomLength);
    }

    protected override void Control()
    {
        if (isGameEnd) return;

        if (targetWord == null || targetWord.Count == 0)
        {
            isGameEnd = true;
            GameSuccess();
            return;
        }

        if (Input.anyKeyDown)
        {
            if (Input.inputString.Length > 0)
            {
                char inputChar = char.ToUpper(Input.inputString[0]);

                if (inputChar != targetWord.Dequeue())
                {
                    isGameEnd = true;
                    GameFail();
                    return;
                }

                SetCorrect(textIndex);
                textIndex++;
            }
        }
    }

    private void GenerateRandomWord(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < length; i++)
        {
            char randomChar = chars[Random.Range(0, chars.Length)];
            targetWord.Enqueue(randomChar);
            targetText[i].gameObject.SetActive(true);
            targetText[i].text = randomChar.ToString();
        }

        parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length * 120);
    }

    private void SetCorrect(int _index)
    {
        targetText[textIndex].color = Color.green;
    }
}
