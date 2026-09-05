using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RecordSlot : MonoBehaviour {
    public Image albumImage;
    public CanvasGroup canvasGroup;

    [Header("难度相关图案")]
    public Image outlineImage;
    public Image circleInImage;
    public Image circleOutImage;

    public void Populate(SongData data) {
        albumImage.sprite = data.albumArt;
        outlineImage.sprite = data.outlineSprite;
        circleInImage.sprite = data.circleInSprite;
        circleOutImage.sprite = data.circleOutSprite;
    }

    public Tween AnimateToCenter(Vector3 targetPos, float bigScale, float duration) {
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOMove(targetPos, duration));
        seq.Join(transform.DOScale(bigScale, duration));
        seq.Join(canvasGroup.DOFade(1f, duration));
        seq.OnComplete(() => SetExtraImagesVisible(true));
        return seq;
    }

    public Tween AnimateToSide(Vector3 targetPos, float smallScale, float duration) {
        SetExtraImagesVisible(false);
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOMove(targetPos, duration));
        seq.Join(transform.DOScale(smallScale, duration));
        seq.Join(canvasGroup.DOFade(0.5f, duration));
        return seq;
    }

    public void SetInstantState(Vector3 pos, float scale, float alpha, bool showExtra) {
        transform.position = pos;
        transform.localScale = Vector3.one * scale;
        canvasGroup.alpha = alpha;
        SetExtraImagesVisible(showExtra);
    }

    private void SetExtraImagesVisible(bool visible) {
        outlineImage.gameObject.SetActive(visible);
        circleInImage.gameObject.SetActive(visible);
        circleOutImage.gameObject.SetActive(visible);
    }
}