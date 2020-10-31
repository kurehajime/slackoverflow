using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace slackoverflow.Models
{
    public class SMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string Comment { get; set; }
        public string Name { get; set; }
        private double _Life = 0.9;
        public double Life
        {
            get
            {
                return _Life;
            }
            set
            {
                _Life = value;
                RaisePropertyChanged(nameof(Life));
            }
        }
    }
}
