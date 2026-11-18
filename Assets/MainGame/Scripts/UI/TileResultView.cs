/*
 * TileResultView
 * --------------
 * The result screen, built on your BaseView framework. It is one BaseView with two SubViews inside it:
 * WinSubView and LoseSubView (tick "Has Subview" and add both to the Sub Views list in the inspector). It does
 * very little by itself: ShowResult remembers whether the player won, opens the view, and once the view has
 * finished appearing (after its own animation, if it has one) OnViewShowDone opens the matching subview.
 * TileResultController decides when to call it, and fills in the texts and buttons.
 */
using com.otm.ui;

public class TileResultView : BaseView
{
    #region Private Variables

    private bool showWin;

    #endregion

    #region Showing

    // Opens the view first, then the win or lose panel inside it.
    public void ShowResult(bool won)
    {
        showWin = won;
        ShowView();
    }

    #endregion

    #region Overridable Hooks

    // Runs once the view has finished appearing: open the win or lose subview.
    protected override void OnViewShowDone()
    {
        if (showWin) ShowSubView<WinSubView>();
        else ShowSubView<LoseSubView>();
    }

    #endregion
}