using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissingSizeFinder.Model
{
    /// <summary>
    /// Base location class, it can be either folder or a drive, shouldnt matter as ong as it has child/parent and we can walk the three
    /// </summary>
    internal class Location
    {
        private string _path;
        private ObservableCollection<Location> _children;

        public ObservableCollection<Location> Children { get { return _children; } }

        internal Location(string path)
        {
            _children = new ObservableCollection<Location>();

            _path = path;
        }
    }
}
