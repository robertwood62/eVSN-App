﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using eLiDAR.Helpers;
using eLiDAR.Models;
using eLiDAR.Views;
using FluentValidation.Results;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using eLiDAR.Utilities;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace eLiDAR.ViewModels {
    public class PlotCommentsViewModel : BasePlotViewModel 
    {
        public PlotCommentsViewModel(INavigation navigation, PLOT _thisplot)
        {
            _navigation = navigation;
            _plot = _thisplot;      
        }
        public string CommentsTitle
        {
            get => "Comments for plot " + _plot.VSNPLOTNAME;
            set
            {
            }
        }

        #region INotifyPropertyChanged    
        public new event PropertyChangedEventHandler PropertyChanged;
        protected new void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
