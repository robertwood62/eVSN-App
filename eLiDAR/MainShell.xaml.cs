using eLiDAR.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace eLiDAR
{
   
    public partial class MainShell : Shell
    {
        readonly Utils utils = new Utils();
        public MainShell()
        {
            try
            {
                BindingContext = this;
                InitializeComponent();
            }
            catch (Exception ex)
            { 
                var error = ex.Message;
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                return utils.IsLoggedIn;
            }
        }

        public bool IsNotLoggedIn
        {
            get
            {
                return !utils.IsLoggedIn;
            }
        }
    }
}