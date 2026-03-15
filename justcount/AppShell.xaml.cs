using justcount.Pages;

namespace justcount;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ExpensePage), typeof(ExpensePage));
    }
}