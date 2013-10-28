using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dbz.UIComponents;

namespace MissingSizeFinder.Model
{
    /// <summary>
    /// Base location class, it can be either folder or a drive, shouldnt matter as ong as it has child/parent and we can walk the three
    /// </summary>
    public class Location : BaseIObservable
    {
        private string _path;
        private ObservableCollection<Location> _children;

        public ObservableCollection<Location> Children { get { return _children; } }
        public string Name { get; set; }
        public string Size { get; set; }
        public double RawSize { get; set; } //Raw size in bytes
        private BackgroundWorker _childLoader;
        private BackgroundWorker _sizeCalculator;

        //This event gets fired once Directory finished loading children and knows its total size
        public event EventHandler LocationFinishedCalculating;

        public Location(string path)
        {
            _children = new ObservableCollection<Location>();

            _path = path;

            Name = _path.Substring(_path.LastIndexOf(@"\") + 1);

            _childLoader = new BackgroundWorker();
            _childLoader.DoWork += _childLoader_DoWork;
            _childLoader.RunWorkerCompleted += _childLoader_RunWorkerCompleted;

            _childLoader.RunWorkerAsync();

            /*
             * Start from the tail ends of trees. The tail ends know about themselves, they have 0 child elements.
                Calculate all files
	                Fire Finished event
		                -------NON tail objects
			                Multiple children? Event is not reliable suka blja
			                event + use the size value. Once all children are loaded size value will not be empty!

             */
            _sizeCalculator = new BackgroundWorker();
            _sizeCalculator.DoWork += _sizeCalculator_DoWork;
            _sizeCalculator.RunWorkerCompleted += _sizeCalculator_RunWorkerCompleted;

            Size = null; //Work out in background worker
        }

        void _sizeCalculator_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            double mbSize = ((double)RawSize / (double)1024) / (double)1024;
            double gbSize = mbSize / (double)1024;

            if ((int)gbSize > 0)
            {
                if (gbSize.ToString().IndexOf(".") > 0)
                    Size = gbSize.ToString().Substring(0, (gbSize.ToString().IndexOf(".") + 2));
                else
                    Size = gbSize.ToString();

                Size += " GB";
            }
            else
            {
                if (mbSize.ToString().IndexOf(".") > 0)
                    Size = mbSize.ToString().Substring(0, (mbSize.ToString().IndexOf(".") + 2));
                else
                    Size = mbSize.ToString();

                Size += " MB";
            }

            RaisePropertyChanged("Size");

            if(LocationFinishedCalculating != null)
                LocationFinishedCalculating(null, null);
        }

        void _sizeCalculator_DoWork(object sender, DoWorkEventArgs e)
        {
            RawSize = 0;

            //Go through all children and assign their size to parent
            foreach (var child in _children)
                RawSize += child.RawSize;

            //Grab size for all file elements in the folder
            DirectoryInfo directoryIfo = new DirectoryInfo(_path);

            try
            {
                foreach (var file in directoryIfo.EnumerateFiles())
                    RawSize += file.Length;
            }
            catch
            {
                //do nothing, rights issues again...
            }
        }

        void item_LocationFinishedCalculating(object sender, EventArgs e)
        {
            //Check if all child elemnts have loaded and fire off size calculator if so
            lock (_sizeCalculator)//This bit can be raised concurrently and in this cases _sizeCalculator can report wrong busy status.
            {
                if (_children.FirstOrDefault(x => x.Size == null) == null && !_sizeCalculator.IsBusy)
                    _sizeCalculator.RunWorkerAsync();
            }
        }

        void _childLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (var item in e.Result as List<Location>)
            {
                _children.Add(item);

                if (string.IsNullOrEmpty(item.Size))
                    item.LocationFinishedCalculating += item_LocationFinishedCalculating;
                else
                    item_LocationFinishedCalculating(null, null);
            } 

            //Fire off size calculation routine if we have no children
            //Otherwise this will be handled by a later event
            if (_children.Count == 0)
                _sizeCalculator.RunWorkerAsync();

        }

        void _childLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Location> children = new List<Location>();

            try
            {

                foreach (var item in Directory.GetDirectories(_path))
                {
                    try
                    {
                        children.Add(new Location(item));
                    }
                    catch
                    {
                        //Do nothing, its probably protected folder. We will deal with this later
                    }
                }

            }
            catch
            {
                //Disregard the constabulary
            }

            e.Result = children;
        }
    }
}
