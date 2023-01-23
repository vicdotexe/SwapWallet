using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swapper.ViewModels.Popups;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms.Xaml;

namespace Swapper.Views.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CoinPicker : Popup
    {
        CoinPickerViewModel _viewModel;
        

        public CoinPicker(long chainId)
        {
            InitializeComponent();
            _viewModel = new CoinPickerViewModel();
            BindingContext = _viewModel;
            _viewModel.ChainId = chainId;
            _viewModel.SetDismiss(Dismiss);
            this.Dismissed += CoinPicker_Dismissed;
        }

        private void CoinPicker_Dismissed(object sender, PopupDismissedEventArgs e)
        {
            BindingContext = null;
            _viewModel.VisibleTokenList.Clear();
            _viewModel.VisibleTokenList = null;
            _viewModel = null;
            this.Dismissed -= CoinPicker_Dismissed;
        }

        public void SetChain(long id)
        {
            _viewModel.ChainId = id;
        }

    }
}