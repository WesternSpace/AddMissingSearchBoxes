using System.ComponentModel;

namespace ClientPlugin.Config
{
    public interface IPluginConfig: INotifyPropertyChanged
    {
        bool FactionsSearchboxEnabled { get; set; }

        bool ChatSearchboxEnabled { get; set; }

        // TODO: Add config properties here, then extend the implementing classes accordingly
    }
}