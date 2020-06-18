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
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        { 
            // setting a website as the url file will force the app into a read-only mode (no ability to add or edit breakpoints)

            if (textSaveFile.Text.Substring(0, 7) == "http://")
            {
                MessageBox.Show("Note: setting the data location to a URL sets the program to a read only mode!");
                editSettings.isURL = true;
            } else
            {
                editSettings.isURL = false;
            }

            editSettings.SaveFile = textSaveFile.Text;
            editSettings.saveSettings();
            bpm._settings = editSettings;
            bpm.LoadXML();
            this.Close();
        }

    }
}
