
using Microsoft.Maui.Controls;
using WijkMeld.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace WijkMeld.App.Views;



public partial class HomeMapView : ContentPage
{
	public HomeMapView()
	{
		InitializeComponent();
    }
    protected override void OnAppearing()
    {
            base.OnAppearing();

            // Controleer of de BindingContext nog niet is ingesteld en of de app context beschikbaar is.
            if (BindingContext == null && Application.Current != null && Application.Current.Handler != null)
            {
                // Haal de ServiceProvider op uit de MAUI context van de huidige applicatie.
                var serviceProvider = Application.Current.Handler.MauiContext?.Services;

                if (serviceProvider != null)
                {
                    // Haal een instantie van HomeMapViewModel op via Dependency Injection.
                    var viewModel = serviceProvider.GetService<HomeMapViewModel>();
    BindingContext = viewModel;

                    // Roep de LoadIncidentsCommand aan zodra de ViewModel is ingesteld en de pagina verschijnt.
                    // Zorg ervoor dat het command uitvoerbaar is.
                    if (viewModel?.LoadIncidentsCommand.CanExecute(null) == true)
                    {
                        viewModel.LoadIncidentsCommand.Execute(null);
                    }
                }
                else
{
    System.Diagnostics.Debug.WriteLine("Fout: ServiceProvider niet beschikbaar in HomeMapView.OnAppearing.");
}
            }
        }
}