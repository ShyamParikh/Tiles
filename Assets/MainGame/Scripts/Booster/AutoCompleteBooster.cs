/*
 * AutoCompleteBooster
 * -------------------
 * Completes one set. It looks at every face on the board and picks the one that needs the fewest tiles: a face
 * already in the tray needs only 1 or 2, otherwise it needs 3. It only picks a set that fits into the free tray
 * slots. The tiles come from anywhere on the board, even covered ones (uncovered tiles first, then the highest
 * layers, so the board is disturbed as little as possible). They are pulled into the tray one after another
 * through the tray manager, and when the last one has arrived the finished set is merged. The charge is taken
 * by BoosterManager afterwards.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public class AutoCompleteBooster : BoosterBase
{
    #region Planning

    // The tiles to pull, or null when no set fits into the free tray slots.
    private List<Tile> BuildPlan()
    {
        List<Tile> board = new List<Tile>(Manager.Board.Tiles);
        Vector2Int bestRun = default;
        int bestInTray = -1, bestNeed = 0;

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

    // Pulls the planned tiles in one after another, then merges the finished set.
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

    #endregion

    #region Abstract Implementations

    // This is the auto complete booster.
    public override BoosterType Type => BoosterType.AutoComplete;

    // There must be a set that fits into the free tray slots.
    public override bool CanExecute()
    {
        return BuildPlan() != null;
    }

    // Builds the plan and pulls the tiles in.
    public override void Execute(Action<bool> onFinished)
    {
        List<Tile> plan = BuildPlan();
        if (plan == null) onFinished(false);
        else PullNext(plan, 0, onFinished);
    }

    #endregion
}