﻿using eLiDAR.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace eLiDAR.Views
{
    public partial class PhotoList : ContentPage
    {

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //     MyListView.ItemsSource = null;

            Device.BeginInvokeOnMainThread(async () =>
            {
                await _viewmodel.CheckTable();
                _viewmodel.FetchVegetation();
            });
            //     MyListView.ItemsSource = _viewmodel.VegetationList;
        }

        private PhotoListViewModel _viewmodel;

        public PhotoList(string plotID)
        {
            InitializeComponent();
            _viewmodel = new PhotoListViewModel(Navigation, plotID);

            this.BindingContext = _viewmodel;
        }
    }
}
