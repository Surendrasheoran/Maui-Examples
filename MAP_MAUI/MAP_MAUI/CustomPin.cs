using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Maps;

namespace MAP_MAUI
{
    public class CustomPin : Pin, INotifyPropertyChanged
    {
        public int PinImage { get; set; }

        public float Angle { get; set; }

        public bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                if (_isPinSelected != null && value == true)
                {
                    _isPinSelected?.Invoke(this, null);
                }

            }
        }

        public EventHandler _isPinSelected;

        public event EventHandler IsPinSelectionChanged
        {
            add
            {
                _isPinSelected += value;
            }

            remove
            {
                _isPinSelected -= value;
            }
        }
    }
}

