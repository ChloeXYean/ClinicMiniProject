using System.Windows.Input;

namespace ClinicMiniProject.UI.Control;
public partial class TopNavBar : ContentView
{
    public static readonly BindableProperty UserNameProperty =
    BindableProperty.Create(nameof(UserName),typeof(string),typeof(TopNavBar),"User");

    public static readonly BindableProperty UserRoleProperty =
        BindableProperty.Create(nameof(UserRole),typeof(string),typeof(TopNavBar),"Doctor");

    public static readonly BindableProperty MenuCommandProperty =
        BindableProperty.Create(nameof(MenuCommand), typeof(ICommand), typeof(TopNavBar), null);

    public static readonly BindableProperty BellCommandProperty =
        BindableProperty.Create(nameof(BellCommand), typeof(ICommand), typeof(TopNavBar), null);

    public string UserName
    {
        get => (string)GetValue(UserNameProperty);
        set => SetValue(UserNameProperty, value);
    }

    public string UserRole
    {
        get => (string)GetValue(UserRoleProperty);
        set => SetValue(UserRoleProperty, value);
    }

    public ICommand? MenuCommand
    {
        get => (ICommand?)GetValue(MenuCommandProperty);
        set => SetValue(MenuCommandProperty, value);
    }

    public ICommand? BellCommand
    {
        get => (ICommand?)GetValue(BellCommandProperty);
        set => SetValue(BellCommandProperty, value);
    }

    public TopNavBar()
    {
        InitializeComponent();
    }

    private void OnMenuClicked(object sender, EventArgs e)
    {
        if (MenuCommand?.CanExecute(null) == true)
            MenuCommand.Execute(null);
    }

	private void OnBellClicked(object sender, EventArgs e)
    {
        if (BellCommand?.CanExecute(null) == true)
            BellCommand.Execute(null);
    }
}