using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Windows;

namespace HaspelPlan.View
{
    public partial class MainPage : ContentPage
    {
        public List<String> JobList;
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new ViewModel.MainViewModel();
        }
    }
}
