using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Tp2WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainViewModel();
            DataContext = viewModel;

            // Passez les arguments à la méthode Initialize
            if (Application.Current is App app && app.Args != null)
            {
                viewModel.Initialize(app.Args);
            }
        }

        private async void BtnListEngr(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel != null)
            {
                await viewModel.FetchData(); 

                var records = viewModel.Records;

                if (records.Count > 0)
                {
                    var firstRecord = records[0];
                    GridView gridView = RecordListView.View as GridView;

                    if (gridView != null)
                    {
                        gridView.Columns.Clear();

                        foreach (var key in firstRecord.Keys)
                        {
                            gridView.Columns.Add(new GridViewColumn
                            {
                                Header = key.ToUpper(),
                                
                                DisplayMemberBinding = new Binding($"[{key}]")
                            });
                        }
                    }
                }
            }
        }
       
        // Gestionnaire de la list d'enregistrement 
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecordListView.SelectedItem is Dictionary<string, string> selectedRecord)
            {
                var message = string.Join("\n", selectedRecord.Select(kv =>
                    kv.Key.Equals("RESULTAT", StringComparison.OrdinalIgnoreCase)
                        ? $"{kv.Key.ToUpper()}: {kv.Value} µg/m³"
                        : $"{kv.Key.ToUpper()}: {kv.Value}"));

                MessageBox.Show($"Enregistrement sélectionné :\n{message}");
            }
        }

    }

}