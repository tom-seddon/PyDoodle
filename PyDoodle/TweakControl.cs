using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PyDoodle
{
    public class TweakControl : UserControl
    {
        public class EventArgs : System.EventArgs
        {
            private object _value;

            public object Value
            {
                get { return _value; }
            }

            public EventArgs(object value)
            {
                _value = value;
            }
        }

        public delegate void OnTweak(object sender, EventArgs ea);

        public OnTweak Tweak;

        public delegate TweakControl TweakControlCreator();

        public static TweakControl CreateTweakControl<T>() where T : TweakControl, new()
        {
            return new T();
        }

        public virtual void SetValue(dynamic newValue)
        {
        }
    }
}
