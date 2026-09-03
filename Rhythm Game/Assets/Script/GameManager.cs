using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;

    private int score;
    private int combo;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(string judgment)
    {
        int baseScore = 0;
        switch (judgment)
        {
            case "Perfect": baseScore = 100; break;
            case "Great": baseScore = 80; break;
            case "Good": baseScore = 50; break;
            case "PerfectHold": baseScore = 150; break;
        }
        combo++;
        score += baseScore * Mathf.Max(1, combo / 10); // 连击加分
        UpdateUI();
    }

    public void BreakCombo()
    {
        combo = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = "Score: " + score;
        if (comboText) comboText.text = combo > 0 ? "Combo: " + combo : "";
    }
}