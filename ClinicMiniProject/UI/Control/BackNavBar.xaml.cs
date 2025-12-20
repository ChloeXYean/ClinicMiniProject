using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace ClinicMiniProject.UI.Control;

public partial class BackNavBar : ContentView
{
    public static readonly BindableProperty BackCommandProperty =
            BindableProperty.Create(nameof(BackCommand), typeof(ICommand), typeof(BackNavBar));

    public ICommand BackCommand
    {
        get => (ICommand)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(BackNavBar), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty TargetRouteProperty =
        BindableProperty.Create(nameof(TargetRoute), typeof(string), typeof(BackNavBar), null);

    public string TargetRoute
    {
        get => (string)GetValue(TargetRouteProperty);
        set => SetValue(TargetRouteProperty, value);
    }

    public BackNavBar()
    {
        InitializeComponent();
    }

    private async void BackBtn_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(TargetRoute))
        {
            await Shell.Current.GoToAsync(TargetRoute);
        }
        else if (BackCommand != null && BackCommand.CanExecute(null))
        {
            BackCommand.Execute(null);
        }
        else
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}