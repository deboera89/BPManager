using BPManager.Classes;
using BPManager.Properties;
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


namespace BPManager
{
    /// <summary>
    /// Interaction logic for settings.xaml
    /// </summary>
    /// 
   
    public partial class settingsWindow : Window
    {
        BPSettings editSettings;
        BreakpointManager bpm;

        public settingsWindow(BreakpointManager bpm)
        {
            InitializeComponent();
            this.bpm = bpm;
            editSettings = this.bpm._settings;
            textSaveFile.Text = editSettings.SaveFile;
            if (editSettings.isURL)
            {
                rdoServerFile.IsChecked = true;
            } else
            {
                rdoLocalFile.IsChecked = true;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // setting a website as the url file will force the app into a read-only mode (no ability to add or edit breakpoints)
            if (rdoServerFile.IsChecked == true && textSaveFile.Text.Substring(0, 7) != "http://")
            {
                MessageBoxResult mb = MessageBox.Show("Error:\r\nIt appears your url isn't valid, must start with 'http://'");
                return;
            }


            if (rdoServerFile.IsChecked == true)
            {

                editSettings.isURL = true;
            }
            else
            {
                editSettings.isURL = false;
            }

            editSettings.SaveFile = textSaveFile.Text;
            editSettings.saveSettings();
            bpm._settings = editSettings;
            bpm.LoadXML();
            this.Close();
        }

        private void RdoServerFile_Checked(object sender, RoutedEventArgs e)
        {
                lblServerWarning.Content = "Note:\r\nServer location sets the program to a read-only mode";
        }

        private void RdoLocalFile_Checked(object sender, RoutedEventArgs e)
        {
            lblServerWarning.Content = "";
        }
    }
}
