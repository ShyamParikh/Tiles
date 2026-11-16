using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Completes one set. Prefers a face already in the tray (pulls only 1 or 2 tiles), otherwise pulls 3.
/// Tiles come from anywhere on the board, even covered ones.
/// </summary>
public class AutoCompleteBooster : BoosterBase
{
    public override BoosterType Type => BoosterType.AutoComplete;

    public override bool CanExecute()
    {
        return BuildPlan() != null;
    }

    public override void Execute(Action<bool> onFinished)
    {
        List<Tile> plan = BuildPlan();
        if (plan == null) onFinished(false);
        else PullNext(plan, 0, onFinished);
    }

    private void PullNext(List<Tile> plan, int index, Action<bool> onFinished)
    {
        if (index == plan.Count)
        {
            Manager.Tray.MergeIfComplete(plan[0].FaceId);
            onFinished(true);
            return;
        }
        Manager.Tray.InsertFromBooster(plan[index], () => PullNext(plan, index + 1, onFinished));
    }

    /// <summary>The tiles to pull, or null when no set fits into the free tray slots.</summary>
    private List<Tile> BuildPlan()
    {
        List<Tile> board = new List<Tile>(Manager.Board.Tiles);
        int bestInTray = -1;
        Vector2Int bestRun = default;
        int bestNeed = 0;

        foreach (Vector2Int run in TileRules.FaceRuns(board))
        {
            int inTray = Manager.Tray.CountOf(board[run.x].FaceId);
            int need = TileRules.SetSize - inTray;
            if (need <= 0 || run.y < need || need > Manager.Tray.FreeSlotCount || inTray <= bestInTray) continue;

            bestInTray = inTray;
            bestRun = run;
            bestNeed = need;
        }

        if (bestInTray < 0) return null;

        // Uncovered tiles first, then the highest layers, so the board is disturbed as little as possible.
        List<Tile> group = board.GetRange(bestRun.x, bestRun.y);
        group.Sort((a, b) => a.IsBlocked != b.IsBlocked ? a.IsBlocked.CompareTo(b.IsBlocked) : b.LayerIndex.CompareTo(a.LayerIndex));
        return group.GetRange(0, bestNeed);
    }
}