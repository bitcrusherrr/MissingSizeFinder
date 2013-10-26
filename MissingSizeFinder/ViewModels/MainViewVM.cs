using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissingSizeFinder.Model;
using System.Collections.ObjectModel;
using dbz.UIComponents;

namespace MissingSizeFinder.ViewModels
{
    internal class MainViewVM : BaseIObservable
    {

        private ObservableCollection<Location> _locationList;
        internal ObservableCollection<Location> MainLocationList { get { return _locationList; } }

        internal MainViewVM()
        {
            _locationList = new ObservableCollection<Location>();


            _locationList.Add(new Location(@"C:\"));

            RaisePropertyChanged("MainLocationList");
        }
    }
}
