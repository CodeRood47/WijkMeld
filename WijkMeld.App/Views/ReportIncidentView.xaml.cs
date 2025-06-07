using Microsoft.Maui.Controls;
using WijkMeld.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;

namespace WijkMeld.App.Views;



public partial class ReportIncidentView : ContentPage
{
	public ReportIncidentView()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
    {
        base.OnAppearing();
        if(BindingContext == null && Application.Current !=null && Application.Current.Handler != null)
        {
           var serviceProvider = Application.Current.Handler.MauiContext?.Services;
           if(serviceProvider != null)
            {
                var viewModel = serviceProvider.GetService<ReportIncidentViewModel>();
                BindingContext = serviceProvider.GetRequiredService<ReportIncidentViewModel>();

                if (viewModel is not null)
                {
                    _ = viewModel.OnAppearingAsync();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ReportIncidentView: ServiceProvider is null, kan ViewModel niet instellen.");
            }
        }

    }
}

       
