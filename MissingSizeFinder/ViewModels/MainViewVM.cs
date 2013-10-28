using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissingSizeFinder.Model;
using System.Collections.ObjectModel;
using dbz.UIComponents;
using System.IO;
using System.Windows.Forms;

namespace MissingSizeFinder.ViewModels
{
    public class MainViewVM : BaseIObservable
    {

        private ObservableCollection<Location> _locationList;
        public ObservableCollection<Location> MainLocationList { get { return _locationList; } }

        private DelegateCommand _rootSelectCommand;
        public DelegateCommand RootSelectCommand { get { return _rootSelectCommand; } }

        public MainViewVM()
        {
            _locationList = new ObservableCollection<Location>();

            _rootSelectCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => SelectBaseFolder()
            };
        }

        private void SelectBaseFolder()
        {
            FolderBrowserDialog browser = new FolderBrowserDialog();
            DialogResult result = browser.ShowDialog();

            if (result == DialogResult.OK)
            {
                _locationList.Clear();

                _locationList.Add(new Location(browser.SelectedPath));
            }
        }
    }
}
