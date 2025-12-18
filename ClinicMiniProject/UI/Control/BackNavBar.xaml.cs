using System.Windows.Input;

namespace ClinicMiniProject.UI.Control;

public partial class BackNavBar : ContentView
{
    public static readonly BindableProperty BackCommandProperty =
            BindableProperty.Create(
                nameof(BackCommand),       // Property Name
                typeof(ICommand),          // Property Type
                typeof(BackNavBar),        // Owning Type
                defaultValue: null);

    public ICommand BackCommand
    {
        get => (ICommand)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public BackNavBar()
    {
        InitializeComponent(); 
    }
}