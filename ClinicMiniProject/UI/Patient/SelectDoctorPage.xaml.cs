using ClinicMiniProject.ViewModels;

namespace ClinicMiniProject.UI.Patient;

public partial class SelectDoctorPage : ContentPage
{
    public SelectDoctorPage(SelectDoctorViewModel viewModel)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("SelectDoctorPage constructor started");
            
            if (viewModel == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: SelectDoctorViewModel is null in constructor");
                throw new ArgumentNullException(nameof(viewModel));
            }
            
            InitializeComponent();
            BindingContext = viewModel;
            
            System.Diagnostics.Debug.WriteLine("SelectDoctorPage constructor completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in SelectDoctorPage constructor: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
