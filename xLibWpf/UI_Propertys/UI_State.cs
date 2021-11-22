using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using xLib;

namespace xLibWpf.UI_Propertys
{
    public class UI_State : NotifyPropertyChanged
    {
        private string name = "";
        private bool state = false;
        private object p_value = "";
        private Brush background;

        private Brush background_true = (Brush)(new BrushConverter().ConvertFrom("#FF641818"));
        private Brush background_false = (Brush)(new BrushConverter().ConvertFrom("#FF21662A"));

        public string Name
        {
            set { name = value; OnPropertyChanged(nameof(Name)); }
            get { return name; }
        }

        public bool State
        {
            set
            {
                state = value;
                Value = state.ToString();
                if (state) { Background = background_true; }
                else { Background = background_false; }
                OnPropertyChanged(nameof(State));
            }
            get { return state; }
        }

        public object Value
        {
            set { p_value = value; OnPropertyChanged(nameof(Value)); }
            get { return p_value; }
        }

        public Brush Background
        {
            set { background = value; OnPropertyChanged(nameof(Background)); }
            get { return background; }
        }

        public string BackgroundTrue
        {
            set { if (value != null) background_true = (Brush)(new BrushConverter().ConvertFrom(value)); }
            get { return background_true.ToString(); }
        }

        public string BackgroundFalse
        {
            set { if (value != null) background_false = (Brush)(new BrushConverter().ConvertFrom(value)); }
            get { return background_false.ToString(); }
        }
    }
}
