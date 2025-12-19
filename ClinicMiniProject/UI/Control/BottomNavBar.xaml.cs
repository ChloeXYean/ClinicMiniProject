using System.Windows.Input;

namespace ClinicMiniProject.UI.Control;

public partial class BottomNavBar : ContentView
{
    public static readonly BindableProperty SelectedTabProperty =
        BindableProperty.Create(nameof(SelectedTab), typeof(string), typeof(BottomNavBar), "Home", propertyChanged: OnSelectedTabChanged);

    public static readonly BindableProperty HomeCommandProperty =
        BindableProperty.Create(nameof(HomeCommand), typeof(ICommand), typeof(BottomNavBar), null);

    public static readonly BindableProperty ChatCommandProperty =
        BindableProperty.Create(nameof(ChatCommand), typeof(ICommand), typeof(BottomNavBar), null);

    public static readonly BindableProperty ProfileCommandProperty =
        BindableProperty.Create(nameof(ProfileCommand), typeof(ICommand), typeof(BottomNavBar), null);

    public BottomNavBar()
    {
        InitializeComponent();
    }

    public string SelectedTab
    {
        get => (string)GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    public ICommand? HomeCommand
    {
        get => (ICommand?)GetValue(HomeCommandProperty);
        set => SetValue(HomeCommandProperty, value);
    }

    public ICommand? ChatCommand
    {
        get => (ICommand?)GetValue(ChatCommandProperty);
        set => SetValue(ChatCommandProperty, value);
    }

    public ICommand? ProfileCommand
    {
        get => (ICommand?)GetValue(ProfileCommandProperty);
        set => SetValue(ProfileCommandProperty, value);
    }

    public bool IsHomeSelected => string.Equals(SelectedTab, "Home", StringComparison.OrdinalIgnoreCase);
    public bool IsChatSelected => string.Equals(SelectedTab, "Chat", StringComparison.OrdinalIgnoreCase);
    public bool IsProfileSelected => string.Equals(SelectedTab, "Profile", StringComparison.OrdinalIgnoreCase);

    private static void OnSelectedTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bar = (BottomNavBar)bindable;
        bar.OnPropertyChanged(nameof(IsHomeSelected));
        bar.OnPropertyChanged(nameof(IsChatSelected));
        bar.OnPropertyChanged(nameof(IsProfileSelected));
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        if (HomeCommand?.CanExecute(null) == true)
            HomeCommand.Execute(null);
    }

    private void OnInquiryClicked(object sender, EventArgs e)
    {
        if (ChatCommand?.CanExecute(null) == true)
            ChatCommand.Execute(null);
    }

    private void OnProfileClicked(object sender, EventArgs e)
    {
        if (ProfileCommand?.CanExecute(null) == true)
            ProfileCommand.Execute(null);
    }
}