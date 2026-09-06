using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public RectTransform targetRect; // 要拖动的物体2(如果脚本直接挂在物体2上,可以在Awake里自动抓自己的RectTransform)

    [Header("上下限制 (基于anchoredPosition的Y值)")]
    public float minY = -300f; // 最低能到多少
    public float maxY = 300f;  // 最高能到多少

    private Vector2 startAnchoredPos;
    private Vector2 startPointerPos;

    void Awake() {
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        startAnchoredPos = targetRect.anchoredPosition;
        startPointerPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) {
        float deltaY = eventData.position.y - startPointerPos.y;
        float newY = startAnchoredPos.y + deltaY;

        // 限制在min~max之间
        newY = Mathf.Clamp(newY, minY, maxY);

        targetRect.anchoredPosition = new Vector2(targetRect.anchoredPosition.x, newY);
    }

    public void OnEndDrag(PointerEventData eventData) {
        // 松手后如果需要什么效果(比如吸附回某个位置),可以在这里加
    }
}