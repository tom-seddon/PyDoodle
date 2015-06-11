using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PyDoodle
{
    public abstract class PythonValue
    {
        private string _valueName;

        public string Name { get { return _valueName; } }

        PythonValue(string valueName)
        {
            _valueName = valueName;
        }

        public abstract bool IsEquivalent(PythonValue rhs);

        public SavedPythonValue GetSavedValue();
    }
}
