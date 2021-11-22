using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using xLib.UI_Propertys;

namespace xLib.Sourse
{
    public class xCommunicationControl : NotifyPropertyChanged
    {
        private Timer timer;
        private int time;
        private int count;
        private bool is_update;
        private Brush background_true = (Brush)(new BrushConverter().ConvertFrom("#FF21662A"));
        private Brush background_false = (Brush)(new BrushConverter().ConvertFrom("#FF641818"));
        //private RequestUI ui_event;

        public Brush Background
        {
            get
            {
                if (IsUpdate) return background_true;
                return background_false;
            }
        }

        public Brush BackgroundTrue
        {
            get { return background_true; }
            set { background_true = value; OnPropertyChanged(nameof(Background)); }
        }

        public Brush BackgroundFalse
        {
            get { return background_false; }
            set { background_false = value; OnPropertyChanged(nameof(Background)); }
        }

        public bool IsUpdate
        {
            get { return is_update; }
            set
            {
                is_update = value;
                OnPropertyChanged(nameof(IsUpdate));
                OnPropertyChanged(nameof(Background));
            }
        }

        public void StartControl(int deadtime)
        {
            count = deadtime;
            time = deadtime;
            timer = new Timer(update_state, null, deadtime, 100);
        }

        public void StopControl() { timer?.Dispose(); }

        private void update_state(object arg)
        {
            if (count > 0) { count -= 100; if (!is_update) { IsUpdate = true; } return; }
            if (is_update) { IsUpdate = false; return; }            
        }

        public void Update() { count = time; }
    }
}
