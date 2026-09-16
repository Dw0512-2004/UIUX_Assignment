using UnityEngine;

public class Hitzone : MonoBehaviour
{
    public int laneIndex; 
    private LongTile activeLongTile = null;

    public void OnPress()
    {
        Tile bestTile = null;
        // 💡 放宽单按音符的最大判定距离 (原来是 1.2，现在改到 1.5)
        float minTileDist = 1.5f; 

        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsInactive.Exclude);
        foreach (Tile t in allTiles)
        {
            if (t.laneIndex == this.laneIndex && !t.IsHit)
            {
                float dist = Mathf.Abs(t.transform.position.y - transform.position.y);
                if (dist < minTileDist)
                {
                    minTileDist = dist;
                    bestTile = t; 
                }
            }
        }

        LongTile bestLongTile = null;
        // 💡 大幅放宽长按音符的最大判定距离 (原来是 1.2，现在改到 1.8，非常容易接住)
        float minLongDist = 1.8f;

        LongTile[] allLongs = FindObjectsByType<LongTile>(FindObjectsInactive.Exclude);
        foreach (LongTile l in allLongs)
        {
            if (l.LaneIndex == this.laneIndex && !l.IsHit)
            {
                float dist = Mathf.Abs(l.transform.position.y - transform.position.y);
                if (dist < minLongDist)
                {
                    minLongDist = dist;
                    bestLongTile = l;
                }
            }
        }

        if (bestTile != null && (bestLongTile == null || minTileDist <= minLongDist))
        {
            // 💡 放宽 Perfect 的范围 (原来 0.6，现在 0.8)
            string judgment = minTileDist < 0.8f ? "Perfect" : "Good";
            int score = minTileDist < 0.8f ? 100 : 50;
            ScoreManager.Instance.AddScore(score, judgment);
            bestTile.OnHit();
        }
        else if (bestLongTile != null)
        {
            // 💡 放宽长按 Perfect 的范围
            string judgment = minLongDist < 0.8f ? "Perfect" : "Good";
            int score = minLongDist < 0.8f ? 100 : 50;
            ScoreManager.Instance.AddScore(score, judgment);
            bestLongTile.OnHeadHit(transform.position.y);
            activeLongTile = bestLongTile; 
        }
    }

    public void OnRelease()
    {
        if (activeLongTile != null)
        {
            activeLongTile.BreakHold();
            activeLongTile = null;
        }
    }
}