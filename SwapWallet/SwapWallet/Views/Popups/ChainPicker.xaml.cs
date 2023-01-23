
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SwapWallet.Services;
using SwapWallet.ViewModels;
using VicWeb;
using VicWeb.Interfaces;
using VicWeb.Misc;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace SwapWallet.Views.Popups
{
	
	public partial class ChainPicker : Popup
	{
		public ObservableCollection<IChain> ChainList { get; set; }


		public ChainPicker()
        {
			InitializeComponent();
			ChainList = new ObservableCollection<IChain>(Locator.Instance.Resolve<IMetaService>().GetChains());
            ChainListView.ItemsSource = ChainList;
            ChainListView.BindingContext = this;
            ChainListView.SetBinding(ListView.SelectedItemProperty, "SelectedChain", BindingMode.TwoWay);

        }

		public MetaChain SelectedChain
        {
            set
            {
                this.Dismiss(value);
            }
        }

        
    }
}