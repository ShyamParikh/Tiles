using com.otm.ui;

// The result screen. It is a BaseView with two SubViews: WinSubView and LoseSubView.
public class TileResultView : BaseView
{
    private bool showWin;

    // Opens the view first, then the win or lose panel inside it.
    public void ShowResult(bool won)
    {
        showWin = won;
        ShowView();
    }

    // Runs once the view has finished appearing (after its animation, if it has one).
    protected override void OnViewShowDone()
    {
        if (showWin) ShowSubView<WinSubView>();
        else ShowSubView<LoseSubView>();
    }
}