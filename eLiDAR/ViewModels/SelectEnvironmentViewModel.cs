using eLiDAR.API;
using eLiDAR.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace eLiDAR.ViewModels
{
    /// <summary>
    /// View model for selecting the environment to use.
    /// </summary>
    public class SelectEnvironmentViewModel : BindableObject
    {
        string selectedEnvironment;

        /// <summary>
        /// Constructs the environment view model.
        /// </summary>
        public SelectEnvironmentViewModel()
        {
            // Get the current custom URL.
            CustomServerUrl = EnvironmentSelector.Environments.FirstOrDefault(e => e.Name == EnvironmentSelector.CustomEnvironmentName).BaseUrl ?? string.Empty;

            // Get the current environment selected.
            selectedEnvironment = EnvironmentSelector.Current.Name;
        }

        /// <summary>
        /// List the available environments. 
        /// </summary>
        public EnvironmentConfig[] Environments { get { return EnvironmentSelector.Environments; } }

        /// <summary>
        /// The currently selected environment.
        /// </summary>
        public EnvironmentConfig SelectedEnvironment
        {
            get
            {
                return Environments.FirstOrDefault(e=>e.Name == selectedEnvironment);
            }
            set
            {
                if(selectedEnvironment != value.Name)
                {
                    selectedEnvironment = value.Name;
                    OnPropertyChanged(nameof(SelectedEnvironment));
                    OnPropertyChanged(nameof(IsCustom));
                }
            }
        }

        /// <summary>
        /// True if the currently selected item is the custom option.
        /// </summary>
        public bool IsCustom
        {
            get
            {
                return selectedEnvironment == EnvironmentSelector.CustomEnvironmentName;
            }
        }

        /// <summary>
        /// The URL to use for custom server backends.
        /// </summary>
        public string CustomServerUrl { get; set; }

        /// <summary>
        /// Save the changes
        /// </summary>
        public ICommand SaveCommand => new Command(async () => await SaveAsync());

        /// <summary>
        /// Saves the changes.
        /// </summary>
        async Task SaveAsync()
        {
            if (IsCustom)
            {
                // Ensure the URL is valid.
                if(!Uri.TryCreate(CustomServerUrl, UriKind.Absolute, out Uri uri))
                {
                    await Application.Current.MainPage.DisplayAlert("Validation Error", "The Server URL provided is not valid.", "OK");
                    return;
                }
                else if(uri.Scheme != "http" && uri.Scheme != "https")
                {
                    await Application.Current.MainPage.DisplayAlert("Validation Error", "The Server URL must begin with http or https.", "OK");
                    return;
                }
                CustomServerUrl = uri.ToString();

                // Ensure sure the URL ends with a slash.
                if(!CustomServerUrl.EndsWith("/"))
                {
                    CustomServerUrl += "/";
                }

                // Save the custom URL.
                EnvironmentSelector.SetCustomEnvironment(CustomServerUrl);
            }

            // Select the current environment.
            EnvironmentSelector.SetEnvironment(SelectedEnvironment.Name);

            // Go back to the previous page.
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}
