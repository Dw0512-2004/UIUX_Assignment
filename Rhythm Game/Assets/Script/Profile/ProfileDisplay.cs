using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileDisplay : MonoBehaviour {
    public TextMeshProUGUI starText;
    public TextMeshProUGUI songText;

    [Header("Grade文字 (S / A / B)")]
    public TextMeshProUGUI gradeSText;
    public TextMeshProUGUI gradeAText;
    public TextMeshProUGUI gradeBText;

    void Start() {
        RefreshProfile();
    }

    void RefreshProfile() {
        if (SaveManager.Instance == null) {
            Debug.LogWarning("SaveManager 还没准备好");
            return;
        }

        // 星星总数
        starText.text = SaveManager.Instance.GetTotalStars().ToString();

        // 解锁歌曲数
        songText.text = SaveManager.Instance.GetUnlockedSongCount().ToString();

        // Grade统计
        var (sCount, aCount, bCount) = SaveManager.Instance.GetGradeCounts();
        gradeSText.text = sCount.ToString();
        gradeAText.text = aCount.ToString();
        gradeBText.text = bCount.ToString();
    }
}