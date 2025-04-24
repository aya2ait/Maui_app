using restaurant.ViewModels;

namespace restaurant.Views
{
    public partial class EditCategoriePage : ContentPage
    {
        private EditCategorieViewModel _viewModel;

        public EditCategoriePage(EditCategorieViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
            
            // Vérifiez si l'URL de l'image est déjà définie au chargement
            UpdateImagePreviewVisibility();
        }
        
        private void OnImageUrlTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateImagePreviewVisibility();
        }
        
        private void UpdateImagePreviewVisibility()
        {
            if (_viewModel?.Categorie?.ImageUrl != null)
            {
                PreviewFrame.IsVisible = !string.IsNullOrWhiteSpace(_viewModel.Categorie.ImageUrl);
            }
            else
            {
                PreviewFrame.IsVisible = false;
            }
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateImagePreviewVisibility();
        }
    }
}