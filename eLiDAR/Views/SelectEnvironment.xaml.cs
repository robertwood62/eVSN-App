using eLiDAR.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace eLiDAR.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectEnvironment : ContentPage
    {
        readonly SelectEnvironmentViewModel viewModel;

        public SelectEnvironment()
        {
            InitializeComponent();
            viewModel = new SelectEnvironmentViewModel();
            BindingContext = viewModel;
        }
    }
}