using eLiDAR.API;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace eLiDAR.ViewModels
{
    /// <summary>
    /// View model for selecting the environment to use.
    /// </summary>
    public class SelectEnvironmentViewModel : BindableObject
    {
        /// <summary>
        /// Constructs the environment view model.
        /// </summary>
        public SelectEnvironmentViewModel()
        {
        }

        /// <summary>
        /// List the available environments. 
        /// </summary>
        public EnvironmentConfig[] Environments
        {
            get
            {
                return EnvironmentSelector.Environments;
            }
        }

        /// <summary>
        /// The currently selected environment.
        /// </summary>
        public EnvironmentConfig Environment
        {
            get
            {
                return EnvironmentSelector.Current;
            }
            set
            {
                EnvironmentSelector.SetEnvironment(value.Name);
                OnPropertyChanged(nameof(Environment));
            }
        }
    }
}
