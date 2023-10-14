using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CopyTemplatesToOpenDocs.Forms
{
    /// <summary>
    /// Interaction logic for ListForm1.xaml
    /// </summary>
    public partial class ListForm1 : Window
    {
        public ListForm1()
        {
            InitializeComponent();
        }

        private void btn_Import_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        //private void CheckBox_Click(object sender, RoutedEventArgs e)
        //{
        //    var checkBox = sender as CheckBox;
        //    if (checkBox != null)
        //    {
        //        // Access the selected item
        //        var selectedItem = dataGrid.SelectedItem as ViewTemplateData;
        //        if (selectedItem != null)
        //        {
        //            // Update the IsSelected property
        //            selectedItem.IsSelected = checkBox.IsChecked == true;
        //        }
        //    }
        //}

    }


    public class ViewTemplateData
    {
        public bool IsSelected { get; set; }
        public string TemplateName { get; set; }

        public ViewTemplateData(string templateName, bool isSelected)
        {
            TemplateName = templateName;
            IsSelected = isSelected;
        }
    }

}
