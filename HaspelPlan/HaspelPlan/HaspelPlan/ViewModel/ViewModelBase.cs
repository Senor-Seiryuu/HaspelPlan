using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HaspelPlan.ViewModelHaspelPlan.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string porpertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(porpertyName));
            }
        }
    }
}
