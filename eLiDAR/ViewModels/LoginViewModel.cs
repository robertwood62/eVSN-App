using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using eLiDAR.Helpers;
using eLiDAR.Models;
using Xamarin.Forms;
using eLiDAR.Utilities;
using System.Windows.Input;
using eLiDAR.API;
using System.Threading.Tasks;
using eLiDAR.Styles;
using eLiDAR.Domain.Global;
using eLiDAR.Views;
using System.Security.Principal;

namespace eLiDAR.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged {

        readonly INavigation _navigation;
        readonly Utils util;

        public ICommand LoginCommand { get; private set; }
        public ICommand SelectEnvironmentCommand { get; private set; }

        public string SelectedTheme { get; set; }

        public LoginViewModel(INavigation navigation)
        {
            _navigation = navigation;
            util = new Utils();
            LoginCommand = new Command(async () => await DoLogin());
            SelectEnvironmentCommand = new Command(async () => await _navigation.PushAsync(new SelectEnvironment()));
        }
       
      

        public event PropertyChangedEventHandler PropertyChanged2;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged2?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
        protected bool SetProperty<T>(ref T backfield, T value, [CallerMemberName]string propertyName =null)
        {
            if (EqualityComparer<T>.Default.Equals(backfield, value))
            {
                return false;
            }
            backfield = value;
            OnPropertyChanged(propertyName);
            return true;
        }
      
        async Task DoLogin()
        {
            try
            {
                // Attempt to login the user and get an access token
                var restService = new RestService();
                var token = await restService.SignInAsync(Username, Pwd);
                if(token == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Login failed", "Incorrect username/password", "OK");
                }
                else
                {
                    util.LoginToken = token.AccessToken;
                    util.LoggedInAs = token.Username;
                    DatabaseHelper.DetachDatabase();
                    Application.Current.MainPage = new MainShell();
                }
            }
            catch (ApplicationException appEx)
            {
                await Application.Current.MainPage.DisplayAlert("Error", appEx.Message, "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                return util.IsLoggedIn;
            }
            set
            {
                //util.IsLoggedIn = value;
                NotifyPropertyChanged("IsLoggedIn");
            }
        }
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                NotifyPropertyChanged("Username");
            }
        }
        private string _pwd;
        public string Pwd
        {
            get => _pwd;
            set
            {
                _pwd = value;
                NotifyPropertyChanged("Pwd");
            }
        }

        #region INotifyPropertyChanged    
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = ""){
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
