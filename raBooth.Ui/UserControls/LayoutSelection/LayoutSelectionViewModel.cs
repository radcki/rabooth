using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using raBooth.Core.Model;
using raBooth.Ui.Model;

namespace raBooth.Ui.UserControls.LayoutSelection
{
    public class LayoutSelectionViewModel : ObservableObject
    {
        public ObservableCollection<SelectableCollageLayout> Layouts { get; set; } = [];
        public event EventHandler<CollageLayoutSelectedEventArgs> LayoutSelected;

        public ICommand SelectLayoutCommand => new RelayCommand<SelectableCollageLayout>(ExecuteSelectLayoutCommand);

        private void ExecuteSelectLayoutCommand(SelectableCollageLayout? layout)
        {
            LayoutSelected?.Invoke(this, new CollageLayoutSelectedEventArgs(layout));
        }

        public void AddLayout(CollageLayout layout)
        {
            App.Current.Dispatcher.Invoke(() =>
                                          {
                                              Layouts.Add(new SelectableCollageLayout(layout));
                                          });
        }
    }

    public record CollageLayoutSelectedEventArgs(SelectableCollageLayout Layout);

}
