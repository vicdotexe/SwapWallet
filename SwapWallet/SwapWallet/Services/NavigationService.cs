using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SwapWallet.ViewModels;
using SwapWallet.Views;
using Xamarin.Forms;

namespace SwapWallet.Services
{
    public class NavigationService
    {
        private readonly IAuthenticationService _authenticationService;
        protected readonly Dictionary<Type, Type> Mappings;
        protected Application CurrentApplication => Application.Current;

        public BaseViewModel GetCurrentViewModel()
        {
            var currentMainpage = (MasterDetailPage)CurrentApplication.MainPage;
            var detailPage = currentMainpage.Detail as NavigationPage;
            var vm = detailPage?.CurrentPage.BindingContext as BaseViewModel;
            return vm;
        }

        public NavigationService(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            Mappings = new Dictionary<Type, Type>();

            CreatePageViewModelMappings();
        }

        public Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel
        {
            return InternalNavigateToAsync(typeof(TViewModel), null);
        }

        public Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : BaseViewModel
        {
            return InternalNavigateToAsync(typeof(TViewModel), parameter);
        }

        public Task NavigateToAsync(Type viewModelType)
        {
            return InternalNavigateToAsync(viewModelType, null);
        }

        public Task NavigateToAsync(Type viewModelType, object parameter)
        {
            return InternalNavigateToAsync(viewModelType, parameter);
        }

        public async Task NavigateBackAsync()
        {
            if (CurrentApplication.MainPage is MainView mainPage)
            {
                await mainPage.Detail.Navigation.PopAsync();
            }
            else if (CurrentApplication.MainPage != null)
            {
                await CurrentApplication.MainPage.Navigation.PopAsync();
            }
        }

        public virtual Task RemoveLastFromBackStackAsync()
        {
            if (CurrentApplication.MainPage is MainView mainPage)
            {
                mainPage.Detail.Navigation.RemovePage(
                    mainPage.Detail.Navigation.NavigationStack[mainPage.Detail.Navigation.NavigationStack.Count - 2]);
            }

            return Task.FromResult(true);
        }

        protected virtual async Task InternalNavigateToAsync(Type viewModelType, object parameter)
        {
            Page page = CreateAndBindPage(viewModelType, parameter);

            switch (page)
            {
                case MainView _:
                    CurrentApplication.MainPage = page;
                    break;
                case LoginView _:
                    CurrentApplication.MainPage = new NavigationPage(page);
                    break;
                default:
                    if (CurrentApplication.MainPage is MainView mainPage)
                    {
                        if (mainPage.Detail is NavigationPage navigationPage &&
                             viewModelType != typeof(AssetsViewModel) &&
                             viewModelType != typeof(LifiSwapViewModel) &&
                             viewModelType != typeof(AccountsViewModel)) //menu items
                        {
                            var currentPage = navigationPage.CurrentPage;

                            if (currentPage.GetType() != page.GetType())
                            {
                                await navigationPage.PushAsync(page, true);
                            }
                        }
                        else
                        {
                            navigationPage = new NavigationPage(page);
                            mainPage.Detail = navigationPage;
                        }

                        var platform = Device.RuntimePlatform == Device.UWP;
                        if (!platform) mainPage.IsPresented = false;
                    }
                    else
                    {
                        if (CurrentApplication.MainPage is NavigationPage navigationPage)
                        {
                            await navigationPage.PushAsync(page, true);
                        }
                        else
                        {
                            CurrentApplication.MainPage = new NavigationPage(page);
                        }
                    }
                    break;
            }

            await (page.BindingContext as BaseViewModel)?.InitializeAsync(parameter);
        }

        protected Type GetPageTypeForViewModel(Type viewModelType)
        {
            if (!Mappings.ContainsKey(viewModelType))
            {
                throw new KeyNotFoundException($"No map for ${viewModelType} was found on navigation mappings");
            }

            return Mappings[viewModelType];
        }

        protected Page CreateAndBindPage(Type viewModelType, object parameter)
        {
            Type pageType = GetPageTypeForViewModel(viewModelType);

            if (pageType == null)
            {
                throw new Exception($"Mapping type for {viewModelType} is not a page");
            }

            Page page = Activator.CreateInstance(pageType) as Page;
            BaseViewModel viewModel = Locator.Instance.Resolve(viewModelType) as BaseViewModel;
            page.BindingContext = viewModel;

            return page;
        }

        private void CreatePageViewModelMappings()
        {
        }
    }
}
