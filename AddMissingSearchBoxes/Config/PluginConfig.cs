using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#if !TORCH

namespace ClientPlugin.Config
{
    public class PluginConfig: IPluginConfig
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetValue<T>(ref T field, T value, [CallerMemberName] string propName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;

            OnPropertyChanged(propName);
        }

        private void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;

            propertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private bool factionsSearchboxEnabled = true;

        public bool FactionsSearchboxEnabled
        {
            get => factionsSearchboxEnabled;
            set => SetValue(ref factionsSearchboxEnabled, value);
        }

        private bool chatSearchboxEnabled = true;

        public bool ChatSearchboxEnabled
        {
            get => chatSearchboxEnabled;
            set => SetValue(ref chatSearchboxEnabled, value);
        }
    }
}

#endif