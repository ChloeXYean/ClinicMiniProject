namespace ClinicMiniProject.UI;

public partial class UITestPage : ContentPage
{
	public string UserName { get; set; }
	public UITestPage(string username)
	{
		InitializeComponent();
		UserName = username;
	}

}