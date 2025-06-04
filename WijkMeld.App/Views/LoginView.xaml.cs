using Microsoft.Maui.Controls;
using WijkMeld.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace WijkMeld.App.Views;

public partial class LoginView : ContentPage
{
	public LoginView()
	{
		InitializeComponent();

		//this.BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

		if(BindingContext == null && Application.Current != null && Application.Current.Handler != null)
		{
			var serviceProvider = Application.Current.Handler.MauiContext?.Services;

			if (serviceProvider != null)
			{
				BindingContext = serviceProvider.GetService<LoginViewModel>();
			}
            else
            {
                System.Diagnostics.Debug.WriteLine("Fout: ServiceProvider niet beschikbaar in LoginView.OnAppearing.");
            }
        }
    }
}