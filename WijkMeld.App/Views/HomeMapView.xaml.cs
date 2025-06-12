
using Microsoft.Maui.Controls;
using WijkMeld.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;
using System.ComponentModel;


namespace WijkMeld.App.Views;



public partial class HomeMapView : ContentPage
{
    private HomeMapViewModel _viewModel;
    public HomeMapView()
	{
		InitializeComponent();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext == null && Application.Current != null && Application.Current.Handler != null)
        {

            var serviceProvider = Application.Current.Handler.MauiContext?.Services;

            if (serviceProvider != null)
            {

                var _viewModel = serviceProvider.GetService<HomeMapViewModel>();
                BindingContext = _viewModel;

                if (_viewModel is not null)
                {
                    _viewModel.PropertyChanged += Vm_propertyChanged;


                    if (_viewModel?.LoadIncidentsCommand.CanExecute(null) == true)
                    {
                        _viewModel.LoadIncidentsCommand.Execute(null);
                    }

                    
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Fout: ServiceProvider niet beschikbaar in HomeMapView.OnAppearing.");
            }
        }
    }

    private void Vm_propertyChanged(object? sender, PropertyChangedEventArgs e)

    {
        if (sender is HomeMapViewModel vm && e.PropertyName == nameof(vm.CurrentMapRegion))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (vm.CurrentMapRegion != null)
                {
                    MyMap.MoveToRegion(vm.CurrentMapRegion);
                }
            });
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Afmelden to prevent events memory leaks 

        // mischien weg halen? 
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= Vm_propertyChanged;
        }
    }

}