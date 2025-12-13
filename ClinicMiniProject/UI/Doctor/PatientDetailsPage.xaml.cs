namespace ClinicMiniProject.UI.Doctor;

[QueryProperty(nameof(PatientIc), "patientIc")]
public partial class PatientDetailsPage : ContentPage
{
    private string _patientIc = string.Empty;

    public string PatientIc
    {
        get => _patientIc;
        set
        {
            _patientIc = value;
            OnPropertyChanged();
        }
    }

    public PatientDetailsPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
}
