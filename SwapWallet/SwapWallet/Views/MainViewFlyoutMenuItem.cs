using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwapWallet.Views
{
    public class MainViewFlyoutMenuItem
    {
        public MainViewFlyoutMenuItem()
        {
            TargetType = typeof(MainViewFlyoutMenuItem);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public Type TargetType { get; set; }
    }
}