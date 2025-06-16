
using WijkMeld.App.ViewModels;
using WijkMeld.App.Converters;
namespace WijkMeld.App.Views;

public partial class IncidentDetailView : ContentPage
{
	public IncidentDetailView(IncidentDetailViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

  
    
}