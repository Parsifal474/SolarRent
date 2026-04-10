namespace SolarRent.Services.Navigation
{
    public interface INavigationService
    {
        void NavigateTo(string pageKey);
        void GoBack();
        bool CanGoBack { get; }
    }
}