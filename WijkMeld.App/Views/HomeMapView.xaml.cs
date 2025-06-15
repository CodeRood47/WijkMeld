
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using WijkMeld.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Maps;


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
                _viewModel = serviceProvider.GetService<HomeMapViewModel>();
                BindingContext = _viewModel;

                if (_viewModel is not null)
                {
                    _viewModel.PropertyChanged += Vm_propertyChanged;
                    _ = _viewModel.OnAppearingAsync();


                    if (_viewModel?.LoadIncidentsCommand.CanExecute(null) == true)
                    {
                        _viewModel.LoadIncidentsCommand.Execute(null);
                    }
                }
            }
            else
            {
               Debug.WriteLine("Fout: ServiceProvider niet beschikbaar in HomeMapView.OnAppearing.");
            }

        }

        MainThread.BeginInvokeOnMainThread(() => {
            Debug.WriteLine("HomeMapView: UpdateToolbarItems aangeroepen in MainThread.");
            UpdateToolbarItems();
        });



    }



    private void Vm_propertyChanged(object? sender, PropertyChangedEventArgs e)
    {

        if (sender is HomeMapViewModel vm)
        {

            if (e.PropertyName == nameof(vm.IsUserRole) ||
                    e.PropertyName == nameof(vm.IsAdminRole) ||
                    e.PropertyName == nameof(vm.IsGuestRole) ||
                    e.PropertyName == nameof(vm.IsFieldAgentRole))
            {
                Debug.WriteLine($"HomeMapView: de user role is {e.PropertyName}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateToolbarItems(); 
                });
            }


            if (e.PropertyName == nameof(HomeMapViewModel.CurrentMapRegion) && vm.CurrentMapRegion != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {

                    Microsoft.Maui.Controls.Maps.Map mapToUse = vm.IsGuestRole ? MyMapGuest : MyMap;


                    if (mapToUse != null)
                    {
                        mapToUse.MoveToRegion(vm.CurrentMapRegion);

                        mapToUse.Pins.Clear();


                        foreach (var pin in vm.IncidentPins)
                        {
                            mapToUse.Pins.Add(pin);
                            Debug.WriteLine($"Pin toegevoegd: {pin.Label} op {pin.Location.Latitude}, {pin.Location.Longitude}");
                         }

                    }












                    //if (vm.IsUserRole && MyMap != null)
                    //{
                    //    MyMap.MoveToRegion(vm.CurrentMapRegion);
                    //}
                    ////else if (vm.IsAdminRole && MyMapAdmin != null)
                    ////{
                    ////    MyMapAdmin.MoveToRegion(vm.CurrentMapRegion);
                    ////}
                    //else if (vm.IsGuestRole && MyMapGuest != null)
                    //{
                    //    MyMapGuest.MoveToRegion(vm.CurrentMapRegion);
                    //}
                    ////else if (vm.IsFieldAgentRole && MyMapFieldAgent != null)
                    ////{
                    ////    MyMapFieldAgent.MoveToRegion(vm.CurrentMapRegion);
                    ////}
                    //else
                    //{
                    //    Debug.WriteLine("HomeMapView: Kaart kan niet verplaatst worden: Geen actieve kaartcontrol of regio is null.");
                    //}
                });
            }

        }
    }



    private void UpdateToolbarItems()
    {
        if (_viewModel == null) return;

        ToolbarItems.Clear();

        ToolbarItems.Add(new ToolbarItem
        {
            Text = "Uitloggen",
            Command = _viewModel.LogoutCommand
        });

        if (_viewModel.IsUserRole || _viewModel.IsGuestRole)
        {
            ToolbarItems.Add(new ToolbarItem
            {
                Text = "Meld Incident",
                Command = _viewModel.NavigateToReportIncidentCommand
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