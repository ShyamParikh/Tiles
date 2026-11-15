using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Completes one set of 3. Prefers a face that already has tiles in the tray (so it pulls only 1 or 2 tiles),
/// otherwise pulls all 3 from the board. Tiles are taken from anywhere, even if they are covered.
/// </summary>
public class AutoCompleteBooster : BoosterBase
{
    private readonly List<Tile> boardTiles = new List<Tile>();

    public override BoosterType Type => BoosterType.AutoComplete;

    public override bool CanExecute()
    {
        return TryBuildPlan(out int _, out List<Tile> _);
    }

    public override void Execute(Action<bool> onFinished)
    {
        if (!TryBuildPlan(out int faceId, out List<Tile> toPull))
        {
            onFinished(false);
            return;
        }

        PullNext(0, faceId, toPull, onFinished);
    }

    private void PullNext(int index, int faceId, List<Tile> toPull, Action<bool> onFinished)
    {
        if (index >= toPull.Count)
        {
            Manager.Tray.TryMergeFace(faceId);
            onFinished(true);
            return;
        }

        Tile tile = toPull[index];
        if (tile == null)
        {
            PullNext(index + 1, faceId, toPull, onFinished);
            return;
        }

        Manager.Tray.InsertTileFromBooster(tile, () => PullNext(index + 1, faceId, toPull, onFinished));
    }

    private bool TryBuildPlan(out int faceId, out List<Tile> toPull)
    {
        faceId = -1;
        toPull = null;

        int perSet = TileMergeData.TilesPerMerge;
        int freeSlots = Manager.Tray.FreeSlotCount;

        Manager.CollectBoardTiles(boardTiles);

        Dictionary<int, List<Tile>> boardByFace = new Dictionary<int, List<Tile>>();
        for (int i = 0; i < boardTiles.Count; i++)
        {
            Tile t = boardTiles[i];
            if (!boardByFace.TryGetValue(t.FaceId, out List<Tile> list))
            {
                list = new List<Tile>();
                boardByFace.Add(t.FaceId, list);
            }
            list.Add(t);
        }

        Dictionary<int, int> trayByFace = new Dictionary<int, int>();
        IReadOnlyList<Tile> trayTiles = Manager.Tray.TrayTiles;
        for (int i = 0; i < trayTiles.Count; i++)
        {
            Tile t = trayTiles[i];
            if (t == null) continue;
            trayByFace.TryGetValue(t.FaceId, out int c);
            trayByFace[t.FaceId] = c + 1;
        }

        int bestInTray = -1;
        int bestFace = -1;

        foreach (KeyValuePair<int, List<Tile>> pair in boardByFace)
        {
            trayByFace.TryGetValue(pair.Key, out int inTray);
            int needed = perSet - inTray;

            if (needed <= 0) continue;
            if (pair.Value.Count < needed) continue;
            if (needed > freeSlots) continue;

            // Prefer faces that already have tiles in the tray (needs fewer pulls).
            if (inTray > bestInTray)
            {
                bestInTray = inTray;
                bestFace = pair.Key;
            }
        }

        if (bestFace < 0) return false;

        List<Tile> candidates = boardByFace[bestFace];
        // Free tiles first, then the highest layers, so the board is disturbed as little as possible.
        candidates.Sort((a, b) =>
        {
            int result = a.IsBlocked.CompareTo(b.IsBlocked);
            if (result != 0) return result;
            return b.LayerIndex.CompareTo(a.LayerIndex);
        });

        int needCount = perSet - bestInTray;
        toPull = new List<Tile>(needCount);
        for (int i = 0; i < needCount; i++) toPull.Add(candidates[i]);

        faceId = bestFace;
        return true;
    }
}