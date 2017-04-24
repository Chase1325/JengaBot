using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using System.Windows.Forms.Layout;
using System.Data;
using System.Dynamic;
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
using System.Windows.Input;


namespace JengaBot
{

    public sealed partial class MainPage : Page
    {
        double ProgressInt = 0;
        int btnWidth = 90;
        int btnHeight = 40;
        string color = "Wheat";

        enum Placement {Left=0,Middle,Right};
        Block[] blocks = Tower.InitializeArray<Block>(54);
        Button[] btn = Tower.InitializeArray<Button>(54);
        Row[] row = Tower.InitializeArray<Row>(18);
        List<Row> rows = new List<Row>();
        
        List<Block> priorityBlocks = new List<Block>();
        double leftTilt = 0; //in mm
        double rightTilt = 0; //in mm
        int placement;
        int selection; //block index selected
        bool moveAbility = false;

        public MainPage()
        {
            this.InitializeComponent();
            rows = row.ToList();
            
            initializeGame();
        }

        void initializeGame()
        {
            setupBtns(btn);
            blocks.Initialize();
            //rows.Initialize();
            
            initializeRows(rows);
            initializeUtility(blocks);
         //   initializeBtns(btn);

            SolverProgress.Value = 0;
            SolverProgress.Maximum = 36;
            ProgressText.Text = ProgressInt.ToString() + "%";
        }

        void initializeRows(List<Row> rows)
        {
            for (int i = 0; i < rows.Count(); i++)
            {
                rows.ElementAt(i).left = 1;
                rows.ElementAt(i).middle = 1;
                rows.ElementAt(i).right = 1;
                rows.ElementAt(i).sum = 3;
            }
        }

        void initializeUtility(Block[] blocks)
        {
            StatusUpdate.Text = " ";

            for (int i = 0; i < 54; i++)
            {
                blocks[i].Index = i + 1;
                blocks[i].Utility = 0;
                btn[i].Content = blocks[i].Utility;
                if (blocks[i].Index % 3 == 0)
                {
                    blocks[i].column = 3;
                }
                else if (blocks[i].Index % 3 == 2)
                {
                    blocks[i].column = 2;
                }
                else if (blocks[i].Index % 3 == 1)
                {
                    blocks[i].column = 1;
                }

                //double subRow = blocks[i].Index / 3;

                blocks[i].row = Math.Ceiling((double)blocks[i].Index / 3);
                //blocks[i].row += 1;

                if (blocks[i].row % 2 == 0)
                {
                    blocks[i].ori = 1;
                }
                else
                {
                    blocks[i].ori = 0;
                }
            }
        }

        void SolveClick(object sender, RoutedEventArgs e)
        {
            
            StatusUpdate.Text = "Solver Activated";

            //Assign Utilities
            intRowUtilities(rows, blocks);
            extRowUtilities(rows, blocks);

            //Sort Blocks based on Utilities
            makePriority(blocks);

            //Send the move command
            sendBlock(priorityBlocks, 0);

            bool signal = false;
            bool probSense = false;
            int  index = 0;

            while(signal == false){

                // wait for status update
                //if Pi command == "Removed", then signal = true;
                //if Pi command == "probeFail", then index +1 and sendBlock(priorityBlocks, index)

                signal = true; //Remove after implemented
            }

            //Decide on optimal place Command & Place
            tiltUtilities(rows, blocks, leftTilt, rightTilt);
            sendPlace(rows, blocks, placement);
            updateBlockPos(blocks, rows, placement);

            //Reset the Utilities
            resetUtilities(blocks);

            //Check if game has been won
        }

        //Internal Row Utilities
        void intRowUtilities(List<Row> rows, Block[] blocks)
        {
            for (int i = 0; i < blocks.Count(); i++)
            {
              
                if ((rows.ElementAt((int)blocks[i].row - 1).sum == 3) && (blocks[i].column == 1 || blocks[i].column == 3))
                {
                    blocks[i].Utility = 50;
                }
                if ((rows.ElementAt((int)blocks[i].row - 1).sum == 3) && (blocks[i].column == 2))
                {
                    blocks[i].Utility = 100;
                }
                if (rows.ElementAt((int)blocks[i].row - 1).sum == 2)
                {
                    if ((rows.ElementAt((int)blocks[i].row - 1).left == 1) && (rows.ElementAt((int)blocks[i].row - 1).right == 1))
                    {
                        blocks[i].Utility = -10000;
                    }
                    else if ((rows.ElementAt((int)blocks[i].row - 1).left == 1) && (rows.ElementAt((int)blocks[i].row - 1).middle == 1))
                    {
                        if (blocks[i].column == 2)
                        {
                            blocks[i].Utility = 25;
                        }
                        else
                        {
                            blocks[i].Utility = -10000;
                        }
                    }
                    else if ((rows.ElementAt((int)blocks[i].row - 1).middle == 1) && (rows.ElementAt((int)blocks[i].row - 1).right == 1))
                    {
                        if (blocks[i].column == 3)
                        {
                            blocks[i].Utility = 25;
                        }
                        else
                        {
                            blocks[i].Utility = -10000;
                        }
                    }
                }
                if (rows.ElementAt((int)blocks[i].row - 1).sum == 1)
                {
                    blocks[i].Utility = -10000;
                }
                if (blocks[i].row == rows.Count())
                {
                    blocks[i].Utility = -10000;
                }
            }
        }

        void extRowUtilities(List<Row> rows, Block[] blocks)
        {
            for (int i = 0; i < rows.Count(); i++)
            {
                for (int c = 0; c < blocks.Count(); c++)
                    if (rows.ElementAt(i).sum == 1)
                    {
                        if (blocks[c].row > i + 1)
                        {
                            if ((blocks[c].ori == 0 && (i + 1) % 2 == 1) || (blocks[c].ori == 1 && (i + 1) % 2 == 0))
                            {
                                blocks[c].Utility -= ((int)blocks[c].row - (i + 1)) * 3;
                            }
                            if ((blocks[c].ori == 0 && (i + 1) % 2 == 0) || (blocks[c].ori == 1 && (i + 1) % 2 == 1))
                            {
                                blocks[c].Utility -= ((int)blocks[c].row - (i + 1)) * 9;
                            }
                        }
                        if (blocks[c].row == i + 1)
                        {
                            blocks[c].Utility += 0;
                        }
                        if (blocks[c].row < i + 1)
                        {
                            blocks[c].Utility += 1;
                        }
                    }
                    else if (rows.ElementAt(i).sum == 2)
                    {
                        if (blocks[c].row > i + 1)
                        {
                            blocks[c].Utility -= ((int)blocks[c].row - (i + 1)) * 2;
                        }
                        if (blocks[c].row == i + 1)
                        {
                            blocks[c].Utility += 0;
                        }
                        if (blocks[c].row < i + 1)
                        {
                            blocks[c].Utility += 1;
                        }

                    }
                    else if (rows.ElementAt(i).sum == 3)
                    {
                        if (blocks[c].row > i + 1)
                        {
                            blocks[c].Utility -= ((int)blocks[c].row - (i + 1));
                        }
                        if (blocks[c].row == i + 1)
                        {
                            blocks[c].Utility += 0;
                        }
                        if (blocks[c].row < i + 1)
                        {
                            blocks[c].Utility += 1;
                        }
                    }
            }
        }

        void makePriority(Block[] blocks)
        {
            StatusUpdate.Text = "";
            priorityBlocks = blocks.OrderByDescending(o => o.Utility).ToList();
            for (int i = 0; i < priorityBlocks.Count; i++)
            {
                btn[i].Content = blocks[i].Utility.ToString();
                StatusUpdate.Text += priorityBlocks[i].Index.ToString() + " " + priorityBlocks[i].Utility.ToString() + " " + priorityBlocks[i].row.ToString() + "; ";
            }
            StatusUpdate.Text += rows.Count().ToString();
        }


        void guiAddRow()
        {
            // Scroll.Height += 40;
            ScrollGrid.Height += 40;

            for (int i = 0; i < 54; i++)
            {
                //Update block positions
                double fart = btn[i].Margin.Top;
                double b = btn[i].Margin.Right;
                double c = btn[i].Margin.Bottom;
                double d = btn[i].Margin.Left;

                fart += 40;
                btn[i].Margin = new Thickness(d, fart, 0, 0);

            }
        }

        void guiMoveBlock(int updateBlock)
        {
          //  if (rows.ElementAt(rows.Count()).sum == 3)
        //    {
                if (blocks[updateBlock - 1].ori == 0)
                {
                    if (placement == 0)
                    {
                        btn[updateBlock - 1].Margin = new Thickness(55, 169 , 0, 0);
                    }
                    if (placement == 1)
                    {
                        btn[updateBlock - 1].Margin = new Thickness(145, 169, 0, 0);
                    }
                    if (placement == 2)
                    {
                        btn[updateBlock - 1].Margin = new Thickness(235, 169, 0, 0);
                    }
                }
                if (blocks[updateBlock - 1].ori == 1)
                {
                    if (placement == 0)
                    {
                    btn[updateBlock - 1].Margin = new Thickness(325, 169, 0, 0);
                }
                if (placement == 1)
                    {
                    btn[updateBlock - 1].Margin = new Thickness(415, 169, 0, 0);

                }
                if (placement == 2)
                    {
                    btn[updateBlock - 1].Margin = new Thickness(505, 169, 0, 0);
                }
            }
          //  }
        }

        void tiltUtilities(List<Row> rows, Block[] blocks, double leftTilt, double rightTilt)
        {
            if (rows.ElementAt(rows.Count() - 1).sum == 3)
            {
                if (rows.Count() % 2 == 0)
                {
                    if (leftTilt > 3)
                    {
                        placement = (int)Placement.Left;
                    }
                    else if (leftTilt < -3)
                    {
                        placement = (int)Placement.Right;
                    }
                    else
                    {
                        placement = (int)Placement.Middle;
                    }
                }
                if (rows.Count() % 2 == 1)
                {
                    if (rightTilt > 3)
                    {
                        placement = (int)Placement.Left;
                    }
                    else if (rightTilt < -3)
                    {
                        placement = (int)Placement.Right;
                    }
                    else
                    {
                        placement = (int)Placement.Middle;
                    }
                }
            }
            if (rows.ElementAt(rows.Count() - 1).sum == 2)
            {
                if (rows.ElementAt(rows.Count() - 1).left == 0)
                {
                    placement = (int)Placement.Left;
                }
                if (rows.ElementAt(rows.Count() - 1).middle == 0)
                {
                    placement = (int)Placement.Middle;
                }
                if (rows.ElementAt(rows.Count() - 1).right == 0)
                {
                    placement = (int)Placement.Right;
                }
            }
            if (rows.ElementAt(rows.Count() - 1).sum == 1)
            {
                if (rows.Count() % 2 == 0)
                {
                    if (rows.ElementAt(rows.Count() - 1).left == 1)
                    {
                        if (leftTilt < 0)
                        {
                            placement = (int)Placement.Right;
                        }
                        else
                        {
                            placement = (int)Placement.Middle;
                        }

                    }
                    if (rows.ElementAt(rows.Count() - 1).middle == 1)
                    {
                        if (leftTilt < 0)
                        {
                            placement = (int)Placement.Right;
                        }
                        else if (leftTilt > 0)
                        {
                            placement = (int)Placement.Left;
                        }
                        else
                        {
                            placement = (int)Placement.Right;
                        }
                    }
                    if (rows.ElementAt(rows.Count() - 1).right == 1)
                    {
                        if (leftTilt > 0)
                        {
                            placement = (int)Placement.Left;
                        }
                        else
                        {
                            placement = (int)Placement.Middle;
                        }
                    }
                }
                if (rows.Count() % 2 == 1)
                {
                    if (rows.ElementAt(rows.Count() - 1).left == 1)
                    {
                        if (rightTilt < 0)
                        {
                            placement = (int)Placement.Right;
                        }
                        else
                        {
                            placement = (int)Placement.Middle;
                        }

                    }
                    if (rows.ElementAt(rows.Count() - 1).middle == 1)
                    {
                        if (rightTilt < 0)
                        {
                            placement = (int)Placement.Right;
                        }
                        else if (rightTilt > 0)
                        {
                            placement = (int)Placement.Left;
                        }
                        else
                        {
                            placement = (int)Placement.Left;
                        }
                    }
                    if (rows.ElementAt(rows.Count() - 1).right == 1)
                    {
                        if (rightTilt > 0)
                        {
                            placement = (int)Placement.Left;
                        }
                        else
                        {
                            placement = (int)Placement.Middle;
                        }
                    }
                }
            }
        }

        void updateBlockPos(Block[] blocks, List<Row> rows, int placement)
        {
       
            if (priorityBlocks[0].column == 1)
            {
                rows.ElementAt((int)priorityBlocks[0].row - 1).left = 0;
                rows.ElementAt((int)priorityBlocks[0].row - 1).sum -= 1;
            }
            if (priorityBlocks[0].column == 2)
            {
                rows.ElementAt((int)priorityBlocks[0].row - 1).middle = 0;
                rows.ElementAt((int)priorityBlocks[0].row - 1).sum -= 1;
            }
            if (priorityBlocks[0].column == 3)
            {
                rows.ElementAt((int)priorityBlocks[0].row - 1).right = 0;
                rows.ElementAt((int)priorityBlocks[0].row - 1).sum -= 1;
            }

            int updateBlock = priorityBlocks[0].Index;
            blocks[updateBlock-1].row = rows.Count();
            blocks[updateBlock-1].column = placement+1;
            if(rows.Count() % 2 == 0)
            {
                blocks[updateBlock - 1].ori = 1;
            } else
            {
                blocks[updateBlock - 1].ori = 0;
            }

            guiMoveBlock(updateBlock);
        }


        void updateBtnBlockPos(Block[] blocks, List<Row> rows, int selection)
        {
            if (blocks[selection].column == 1)
            {
                rows.ElementAt((int)blocks[selection].row - 1).left = 0;
                rows.ElementAt((int)blocks[selection].row - 1).sum -= 1;
            }
            if (blocks[selection].column == 2)
            {
                rows.ElementAt((int)blocks[selection].row - 1).middle = 0;
                rows.ElementAt((int)blocks[selection].row - 1).sum -= 1;
            }
            if (blocks[selection].column == 3)
            {
                rows.ElementAt((int)blocks[selection].row - 1).right = 0;
                rows.ElementAt((int)blocks[selection].row - 1).sum -= 1;
            }

            int updateBlock = blocks[selection].Index;
            blocks[updateBlock - 1].row = rows.Count();
            blocks[updateBlock - 1].column = placement;
            if (rows.Count() % 2 == 0)
            {
                blocks[updateBlock - 1].ori = 1;
            }
            else
            {
                blocks[updateBlock - 1].ori = 0;

            }
            guiMoveBlock(updateBlock);
        }

        void resetUtilities(Block[] blocks)
        {
            for(int i=0; i<blocks.Count(); i++)
            {
                blocks[i].Utility = 0;
            }

            placement = 0;
            selection = 0;
        }


        void sendPlace(List<Row> rows, Block[] blocks, int placement)
        {
            if(rows.ElementAt(rows.Count()-1).sum == 3)
            {
                guiAddRow();

                if(placement == 0)
                {
                    //send pi command of int placement
                    rows.Add(new Row() { left = 1, middle = 0, right = 0, sum = 1 });

                }
                if (placement == 1)
                {
                    //send pi command of int placement
                    rows.Add(new Row() { left = 0, middle = 1, right = 0, sum = 1 });

                }
                if (placement == 2)
                {
                    //send pi command of int placement
                    rows.Add(new Row() { left = 0, middle = 0, right = 1, sum = 1 });

                }
            } else if(rows.ElementAt(rows.Count() - 1).sum == 2)
            {
                if (placement == 0) {
                    rows.ElementAt(rows.Count()-1).left = 1;
                    rows.ElementAt(rows.Count()-1).sum = 3;
                }
                if (placement == 1)
                {
                    rows.ElementAt(rows.Count()-1).middle = 1;
                    rows.ElementAt(rows.Count()-1).sum = 3;
                }

                if (placement == 2)
                {
                    rows.ElementAt(rows.Count()-1).right = 1;
                    rows.ElementAt(rows.Count()-1).sum = 3;
                }
                } else if ((rows.ElementAt(rows.Count() - 1).sum == 1))
            {
                if (placement == 0)
                {
                    rows.ElementAt(rows.Count()-1).left = 1;
                    rows.ElementAt(rows.Count()-1).sum = 2;
                }
                if (placement == 1)
                {
                    rows.ElementAt(rows.Count()-1).middle = 1;
                    rows.ElementAt(rows.Count()-1).sum = 2;
                }

                if (placement == 2)
                {
                    rows.ElementAt(rows.Count()-1).right = 1;
                    rows.ElementAt(rows.Count()-1).sum = 2;
                }
            }
        }
       

        void sendBlock(List<Block> priorityBlock, int index)
        {
            //Send priorityBlocks[index] to Pi
        }

       void piGameStatus()
        {

        }

        void piTilt()
        {

        }

      
        void PlayMoveClick(object sender, RoutedEventArgs e)
        {
        
           if(moveAbility == true && blocks[selection].row != rows.Count())
            {
                userMove(selection);
                StatusUpdate.Text = "User Played a Move";
                moveAbility = false;
                this.InitializeComponent();
            }

        }


        void didWin()
        {
            for(int i = 0; i < rows.Count(); i++)
            {
                Popup winPop = new Popup();
                TextBlock popupText = new TextBlock();
                popupText.Text = "YOU WIN";
                winPop.Child = popupText;
            }
        }
    
        void didLoose()
        {

        }

        void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
        void textBlock_Copy2_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
        void CurrentStatus_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        void setupBtns(Button[] btn)
        {
            btn[0] = B0;
            btn[1] = B1;
            btn[2] = B2;
            btn[3] = B3;
            btn[4] = B4;
            btn[5] = B5;
            btn[6] = B6;
            btn[7] = B7;
            btn[8] = B8;
            btn[9] = B9;
            btn[10] = B10;
            btn[11] = B11;
            btn[12] = B12;
            btn[13] = B13;
            btn[14] = B14;
            btn[15] = B15;
            btn[16] = B16;
            btn[17] = B17;
            btn[18] = B18;
            btn[19] = B19;
            btn[20] = B20;
            btn[21] = B21;
            btn[22] = B22;
            btn[23] = B23;
            btn[24] = B24;
            btn[25] = B25;
            btn[26] = B26;
            btn[27] = B27;
            btn[28] = B28;
            btn[29] = B29;
            btn[30] = B30;
            btn[31] = B31;
            btn[32] = B32;
            btn[33] = B33;
            btn[34] = B34;
            btn[35] = B35;
            btn[36] = B36;
            btn[37] = B37;
            btn[38] = B38;
            btn[39] = B39;
            btn[40] = B40;
            btn[41] = B41;
            btn[42] = B42;
            btn[43] = B43;
            btn[44] = B44;
            btn[45] = B45;
            btn[46] = B46;
            btn[47] = B47;
            btn[48] = B48;
            btn[49] = B49;
            btn[50] = B50;
            btn[51] = B51;
            btn[52] = B52;
            btn[53] = B53;
        }
        /// <summary>
        /// Button Handlers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        void userMove(int x)
        {
            selection = x;
            sendPlace(rows, blocks, placement);
            updateBtnBlockPos(blocks, rows, selection);
            for (int i = 0; i < blocks.Count(); i++)
            {
                StatusUpdate.Text += blocks[i].Utility.ToString() + " " + blocks[i].Index.ToString() + "; ";
            }
            StatusUpdate.Text += rows.Count().ToString();
            resetUtilities(blocks);
        }

        private void B0_Click(object sender, RoutedEventArgs e)
        {
            selection = 0;
            //;
        }

        private void B1_Click(object sender, RoutedEventArgs e)
        {
            selection = 1;
            //;
        }

        private void B2_Click(object sender, RoutedEventArgs e)
        {
            selection = 2;
            //;
        }

        private void B3_Click(object sender, RoutedEventArgs e)
        {
            selection = 3;
            //;
        }

        private void B4_Click(object sender, RoutedEventArgs e)
        {
            selection = 4;
            //;
        }

        private void B5_Click(object sender, RoutedEventArgs e)
        {
            selection = 5;
            //;
        }

        private void B6_Click(object sender, RoutedEventArgs e)
        {
            selection = 6;
            //;
        }

        private void B7_Click(object sender, RoutedEventArgs e)
        {
            selection = 7;
            //;
        }

        private void B8_Click(object sender, RoutedEventArgs e)
        {
            selection = 8;
            //;
        }

        private void B9_Click(object sender, RoutedEventArgs e)
        {
            selection = 9;
            //;
        }

        private void B10_Click(object sender, RoutedEventArgs e)
        {
            selection = 10;
            //;
        }

        private void B11_Click(object sender, RoutedEventArgs e)
        {
            selection = 11;
            //;
        }

        private void B12_Click(object sender, RoutedEventArgs e)
        {
            selection = 12;
            //;
        }

        private void B13_Click(object sender, RoutedEventArgs e)
        {
            selection = 13;
            //;
        }

        private void B14_Click(object sender, RoutedEventArgs e)
        {
            selection = 14;
            //;
        }

        private void B15_Click(object sender, RoutedEventArgs e)
        {
            selection = 15;
            //;
        }

        private void B16_Click(object sender, RoutedEventArgs e)
        {
            selection = 16;
            //;
        }

        private void B17_Click(object sender, RoutedEventArgs e)
        {
            selection = 17;
            //;
        }

        private void B18_Click(object sender, RoutedEventArgs e)
        {
            selection = 18;
            //;
        }

        private void B19_Click(object sender, RoutedEventArgs e)
        {
            selection = 19;
            //;
        }

        private void B20_Click(object sender, RoutedEventArgs e)
        {
            selection = 20;
            //;
        }

        private void B21_Click(object sender, RoutedEventArgs e)
        {
            selection = 21;
            //;
        }

        private void B22_Click(object sender, RoutedEventArgs e)
        {
            selection = 22;
            //;
        }

        private void B23_Click(object sender, RoutedEventArgs e)
        {
            selection = 23;
            //;
        }

        private void B24_Click(object sender, RoutedEventArgs e)
        {
            selection = 24;
            //;
        }

        private void B25_Click(object sender, RoutedEventArgs e)
        {
            selection = 25;
            //;
        }

        private void B26_Click(object sender, RoutedEventArgs e)
        {
            selection = 26;
            //;
        }

        private void B27_Click(object sender, RoutedEventArgs e)
        {
            selection = 27;
            //;
        }

        private void B28_Click(object sender, RoutedEventArgs e)
        {
            selection =28;
            //;
        }

        private void B29_Click(object sender, RoutedEventArgs e)
        {
            selection = 29;
            //;
        }

        private void B30_Click(object sender, RoutedEventArgs e)
        {
            selection = 30;
            //;
        }

        private void B31_Click(object sender, RoutedEventArgs e)
        {
            selection = 31;
            //;
        }

        private void B32_Click(object sender, RoutedEventArgs e)
        {
            selection = 32;
            //;
        }

        private void B33_Click(object sender, RoutedEventArgs e)
        {
            selection = 33;
            //;
        }

        private void B34_Click(object sender, RoutedEventArgs e)
        {
            selection = 34;
            //;
        }

        private void B35_Click(object sender, RoutedEventArgs e)
        {
            selection = 35;
            //;
        }

        private void B36_Click(object sender, RoutedEventArgs e)
        {
            selection = 36;
            //;
        }

        private void B37_Click(object sender, RoutedEventArgs e)
        {
            selection = 37;
            //;
        }

        private void B38_Click(object sender, RoutedEventArgs e)
        {
            selection = 38;
            //;
        }

        private void B39_Click(object sender, RoutedEventArgs e)
        {
            selection = 39;
            //;
        }

        private void B40_Click(object sender, RoutedEventArgs e)
        {
            selection = 40;
            //;
        }

        private void B41_Click(object sender, RoutedEventArgs e)
        {
            selection = 41;
            //;
        }

        private void B42_Click(object sender, RoutedEventArgs e)
        {
            selection = 42;
            //;
        }

        private void B43_Click(object sender, RoutedEventArgs e)
        {
            selection = 43;
            //;
        }

        private void B44_Click(object sender, RoutedEventArgs e)
        {
            selection = 44;
            //;
        }

        private void B45_Click(object sender, RoutedEventArgs e)
        {
            selection = 45;
            //;
        }

        private void B46_Click(object sender, RoutedEventArgs e)
        {
            selection = 46;
            //;
        }

        private void B47_Click(object sender, RoutedEventArgs e)
        {
            selection = 47;
            //;
        }

        private void B48_Click(object sender, RoutedEventArgs e)
        {
            selection = 48;
            //;
        }

        private void B49_Click(object sender, RoutedEventArgs e)
        {
            selection = 49;
            //;
        }

        private void B50_Click(object sender, RoutedEventArgs e)
        {
            selection = 50;
            //;
        }

        private void B51_Click(object sender, RoutedEventArgs e)
        {
            selection = 51;
            //;
        }

        private void B52_Click(object sender, RoutedEventArgs e)
        {
            selection = 52;
            //;
        }

        private void B53_Click(object sender, RoutedEventArgs e)
        {
            selection = 53;
            //;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }

        private void textBlock_Copy10_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            if(rows.ElementAt(rows.Count()-1).left == 1 && rows.ElementAt(rows.Count()-1).sum != 3)
            {
                StatusUpdate.Text = "Invalid Selection";
                moveAbility = false;
            }
            else
            {
                StatusUpdate.Text = "Left Position Selected";
                placement = 0;
                moveAbility = true;
            }
        }

        private void Middle_Click(object sender, RoutedEventArgs e)
        {
            if (rows.ElementAt(rows.Count()-1).middle == 1 && rows.ElementAt(rows.Count() - 1).sum != 3)
            {
                StatusUpdate.Text = "Invalid Selection";
                moveAbility = false;
            }
            else
            {
                StatusUpdate.Text = "Middle Position Selected";
                placement = 1;
                moveAbility = true;
            }
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            if (rows.ElementAt(rows.Count()-1).right == 1 && rows.ElementAt(rows.Count() - 1).sum != 3)
            {
                StatusUpdate.Text = "Invalid Selection";
                moveAbility = false;
            }
            else
            {
                StatusUpdate.Text = "Right Position Selected";
                placement = 2;
                moveAbility = true;
            }
        }
    }
}
