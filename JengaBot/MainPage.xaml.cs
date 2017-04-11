using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using JengaBot.Models;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace JengaBot
{
  
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Blocks[] blockArray = new Blocks[54];

            if (ViewSelect.IsOn)
            {

                ViewName.Text = "Left View";
                //Show Left View
            } else if (!ViewSelect.IsOn)
            {
                ViewName.Text = "Right View";
                //Show Right View
            }
        }

        void SolveClick(object sender, RoutedEventArgs e)
        {
            //TODO: execute code to play jenga
            StatusUpdate.Text = "Solver Activated";
        }

        void PlayMoveClick(object sender, RoutedEventArgs e)
        {
            //TODO: Execute the move if valid selection
            StatusUpdate.Text = "User Played a Move";
           
        }

        void SwitchView(object sender, RoutedEventArgs e) {
            if (ViewSelect.IsOn)
            {
                StatusUpdate.Text = "Left View Enabled";
                ViewName.Text = "Left View";
                //SHOW the left view
            }

            if (!ViewSelect.IsOn)
            {
                StatusUpdate.Text = "Right View Enabled";
                ViewName.Text = "Right View";
                //Show the right view
            }
        }
        

        void CurrentStatus_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        void leftView()
        {
            
        }

        void rightView()
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void gridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
