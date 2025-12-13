namespace ClinicMiniProject.UI.Control;

public partial class BackNavBar : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(BackNavBar), "Page Title", propertyChanged: OnTitleChanged);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (BackNavBar)bindable;
        control.TitleLabel.Text = (string)newValue;
    }

    public BackNavBar()
    {
        InitializeComponent();
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        BackClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler BackClicked;
}