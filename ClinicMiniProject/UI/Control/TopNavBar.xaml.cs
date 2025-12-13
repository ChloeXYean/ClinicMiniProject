namespace ClinicMiniProject.UI.Control;
public partial class TopNavBar : ContentView
{
    public static readonly BindableProperty UserNameProperty =
    BindableProperty.Create(nameof(UserName),typeof(string),typeof(TopNavBar),"User");

    public string UserName
    {
        get => (string)GetValue(UserNameProperty);
        set => SetValue(UserNameProperty, value);
    }

    public TopNavBar()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public string UserGreeting => $"Hi, {UserName}!";

    private void OnMenuClicked(object sender, EventArgs e) { }

	private void OnBellClicked(object sender, EventArgs e) { }
}