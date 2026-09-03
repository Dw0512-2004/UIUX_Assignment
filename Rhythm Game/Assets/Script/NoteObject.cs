using UnityEngine;
using UnityEngine.UI;

public class NoteObject : MonoBehaviour
{
    public float targetTime;
    public int trackIndex;
    public NoteType noteType = NoteType.Normal;   // 使用全局 NoteType
    public float holdDuration;

    private RectTransform rect;
    private Image image;
    private bool isJudged = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void Setup(float time, int track, NoteType type = NoteType.Normal, float duration = 0f)
    {
        targetTime = time;
        trackIndex = track;
        noteType = type;
        holdDuration = duration;
        isJudged = false;

        if (type == NoteType.Hold)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40 + duration * 200);
        }
        else
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40);
        }

        gameObject.SetActive(true);
    }

    public void UpdatePosition(float currentSongTime, float speed, float hitLineY)
    {
        float distance = (targetTime - currentSongTime) * speed;
        float y = hitLineY + distance;
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);

        if (!isJudged && y < hitLineY - 30f)
        {
            Miss();
        }
    }

    public void OnHitDown(float hitTime)
    {
        if (isJudged) return;

        float delta = Mathf.Abs(hitTime - targetTime);
        float perfect = 0.06f;
        float great = 0.12f;
        float good = 0.18f;

        if (delta <= perfect) Judge("Perfect");
        else if (delta <= great) Judge("Great");
        else if (delta <= good) Judge("Good");
        else Miss();
    }

    public void OnHitUp(float releaseTime)
    {
        if (isJudged || noteType != NoteType.Hold) return;

        float expectedEnd = targetTime + holdDuration;
        if (releaseTime >= expectedEnd - 0.2f && releaseTime <= expectedEnd + 0.2f)
        {
            Judge("PerfectHold");
        }
        else
        {
            Miss();
        }
    }

    void Judge(string judgment)
    {
        if (isJudged) return;
        isJudged = true;
        GameManager.Instance.AddScore(judgment);
        ReturnToPool();
    }

    void Miss()
    {
        if (isJudged) return;
        isJudged = true;
        GameManager.Instance.BreakCombo();
        ReturnToPool();
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false);
        NoteManager.Instance.ReturnToPool(this);
    }
}