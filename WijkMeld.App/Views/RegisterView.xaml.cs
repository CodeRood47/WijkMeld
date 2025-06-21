namespace WijkMeld.App.Views;

using WijkMeld.App.ViewModels;

public partial class RegisterView : ContentPage
{
	public RegisterView(RegisterViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}