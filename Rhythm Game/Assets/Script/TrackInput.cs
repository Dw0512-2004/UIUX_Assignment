using UnityEngine;
using UnityEngine.EventSystems;

public class TrackInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int trackIndex;
    private bool isPressed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!NoteManager.Instance) return;
        isPressed = true;

        NoteObject note = NoteManager.Instance.GetClosestUnjudged(trackIndex);
        if (note != null)
        {
            float hitTime = NoteManager.Instance.CurrentSongTime;
            note.OnHitDown(hitTime);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;
        isPressed = false;

        NoteObject note = NoteManager.Instance.GetClosestUnjudged(trackIndex);
        if (note != null && note.noteType == NoteType.Hold)
        {
            float releaseTime = NoteManager.Instance.CurrentSongTime;
            note.OnHitUp(releaseTime);
        }
    }
}