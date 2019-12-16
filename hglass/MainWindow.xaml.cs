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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hglass
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int cmax;
        private int rmax;
        private int part_size;
        private int timer_dur = 50;

        System.Windows.Threading.DispatcherTimer tmr1;
        private RenderTargetBitmap bmpWater;
        private int[,] hourglass;
        private bool hourglassDone = false;
        private bool simulationPaused = false;
        private bool simulationStarted = false;
        private bool IsDrawingNow = false;
        static System.ComponentModel.BackgroundWorker bkw1 = new System.ComponentModel.BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            tmr1 = new System.Windows.Threading.DispatcherTimer();
            txtSpeed.Content = timer_dur.ToString() + " ms";
        }

        // catches our spacebar keypress to exit the hourglass application
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                Application.Current.Shutdown();
            }
            e.Handled = true;
        }

        // Menu functionality
        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Menu functionality
        private void mnuHourglassShape_Click(object sender, RoutedEventArgs e)
        {
            createHourglass();
        }

        // Menu functionality
        private void mnuSieveShape_Click(object sender, RoutedEventArgs e)
        {
            createSieveShape();
        }

        // Menu functionality
        private void mnuHelp_Click(object sender, RoutedEventArgs e)
        {
            string str = "";
            str += "***************************************************\n";
            str += "SHAPE MENU\n";
            str += "-- Hourglass mode -- draws a conventional hourglass\n";
            str += "-- Sieve mode -- draws a barrier with multiple orifices.\n";
            str += "***************************************************\n";
            MessageBox.Show(str);
        }

        // Menu functionality
        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            string str = "";
            str += "Hourglass and Sieve Simulator v1.0\n";
            str += "Copyright 2018\n";
            str += "Jim Allen\n";
            MessageBox.Show(str);
        }

        // function that creates the array for the hourglass data
        private void createHourglass()
        {
            {
                // initialize values of the array as empty '0'
                for (int r = 0; r < rmax; r++)
                    for (int c = 0; c < cmax; c++)
                        hourglass[r, c] = 0;

                // load the top half of the array with sand '1'
                for (int r = rmax - 1; r >= rmax / 2; r--)
                    //            for (r = 0; r < rmax / 2; r++)
                    for (int c = 0; c < cmax; c++)
                        hourglass[r, c] = 1;

                // load the boundary members...
                int count = cmax / 2 - 1;
                int i = 1;
                while (count >= 0)
                {
                    for (int c = 0; c <= count; c++)
                    {
                        if ((rmax / 2 - 1 - i) >= 0)
                            hourglass[rmax / 2 - 1 - i, c] = 2;
                        if ((rmax / 2 + i) < rmax)
                            hourglass[rmax / 2 + i, c] = 2;
                        if ((rmax / 2 - 1 - i) >= 0)
                            hourglass[rmax / 2 - 1 - i, cmax - 1 - c] = 2;
                        if ((rmax / 2 + i) < rmax)
                            hourglass[rmax / 2 + i, cmax - 1 - c] = 2;
                    }

                    count--;
                    i++;
                }

                // Now cut a hole in the middle section to make sure we have an opening
                for (int r = rmax / 2 - 1; r <= rmax / 2 + 1; r++)
                    for (int c = 0; c < cmax; c++)
                    {
                        if ((cmax % 2) == 0)                // even number of columns
                        {
                            if ((c == cmax / 2) || (c == cmax / 2 -1))
                            {
                                if ((r - 1) > 0)
                                    hourglass[r - 1, c] = 0;
                                hourglass[r, c] = 0;
                            }
                            else
                            {
                                hourglass[r, c] = 2;
                            }
                        } else
                        {
                            if (c == cmax / 2)              // odd number of columns
                            {
                                if ((r - 1) > 0)
                                    hourglass[r - 1, c] = 0;
                                hourglass[r, c] = 0;
                            }
                            else
                            {
                                hourglass[r, c] = 2;
                            }
                        }

                    }
                imgWater.Width = cmax * part_size;
                imgWater.Height = rmax * part_size;

                displayHourglass();
            }
        }

        // constructs the array four our sieve simulator
        private void createSieveShape()
        {
            // initialize values of the array as empty '0'
            for (int r = 0; r < rmax; r++)
                for (int c = 0; c < cmax; c++)
                    hourglass[r, c] = 0;

            // load the top half of the array with sand '1'
            for (int r = rmax - 1; r >= rmax / 2; r--)
                //            for (r = 0; r < rmax / 2; r++)
                for (int c = 0; c < cmax; c++)
                    hourglass[r, c] = 1;

            // load the boundary members...
            for (int r = rmax / 2 - 1; r < rmax / 2 + 1; r++)
                for (int c = 0; c < cmax; c++)
                {
                    //if ((c == cmax / 2))
                    if ((c == cmax / 2) || (c == cmax / 4) || (c == 3*cmax / 4) || (c==0) || (c==cmax-1))
                    //if ((c == 3 * cmax / 4) || (c == 0) || (c == cmax - 1))
                        hourglass[r, c] = 0;
                    else
                        hourglass[r, c] = 2;
                }

            imgWater.Width = cmax * part_size;
            imgWater.Height = rmax * part_size;

            displayHourglass();
        }

        // event handler
        private void OnMouseEnterHandler(object sender, RoutedEventArgs e)
        {
            if(simulationStarted)
            {
                controlsPanel.Visibility = Visibility.Visible;
            }
            e.Handled = true;
        }

        // event handler
        private void OnMouseLeaveHandler(object sender, RoutedEventArgs e)
        {
            if (simulationStarted)
            {
                controlsPanel.Visibility = Visibility.Hidden;
            }
            e.Handled = true;
        }

        // decrements speed of the simulator
        private void btnSlower_Click(object sender, RoutedEventArgs e)
        {
            tmr1.Tick -= new EventHandler(Tmr_Tick);   // remove our tick timer
            int incr = 20;

            if (timer_dur + incr < 200)
            {
                timer_dur += 20;
            } else
            {
                timer_dur = 200;
            }
            tmr1.Tick += new EventHandler(Tmr_Tick);   // install our new tick timer
            tmr1.Interval = new System.TimeSpan(0, 0, 0, 0, timer_dur);
            tmr1.Start();
            txtSpeed.Content = timer_dur.ToString() + " ms";
        }

        // increase speed of the simulator
        private void btnFaster_Click(object sender, RoutedEventArgs e)
        {
            tmr1.Tick -= new EventHandler(Tmr_Tick);   // remove our tick timer
            int incr = 5;

            if(timer_dur - incr > 5)
            {
                timer_dur -= incr;
            } else
            {
                timer_dur = 5;
            }
            tmr1.Tick += new EventHandler(Tmr_Tick);   // install our new tick timer
            tmr1.Interval = new System.TimeSpan(0, 0, 0, 0, timer_dur);
            tmr1.Start();
            txtSpeed.Content = timer_dur.ToString() + " ms";
        }

        // Reads our hourglass input data from combo boxes and displays the initial hourglass
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string str = cmbWidth.Items[cmbWidth.SelectedIndex].ToString();
            int cval;
            int rval;
            int partsize_val;
            bool parseOK = false;

            var item = (ComboBoxItem)cmbWidth.SelectedValue;
            var content = (string)item.Content;
            parseOK = Int32.TryParse(content, out cval);
            if(parseOK)
                cmax=cval;

            item = (ComboBoxItem)cmbHeight.SelectedValue;
            content = (string)item.Content;
            parseOK = Int32.TryParse(content, out rval);
            if (parseOK)
                rmax = rval;

            item = (ComboBoxItem)cmbParticleSize.SelectedValue;
            content = (string)item.Content;
            parseOK = Int32.TryParse(content, out partsize_val);
            if (parseOK)
                part_size = partsize_val;

            hourglass = new int[rmax, cmax];

            dataPanel.Visibility = Visibility.Collapsed;
            mainPanel.Visibility = Visibility.Visible;
            controlsPanel.Visibility = Visibility.Visible;
            createHourglass();
        }

        // starts the simulator after inout data has been applied
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            rotateHourglass();
            simulationStarted = true;
        }

        // intrrupts and restarts the simulator
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            simulationStarted = false;
            tmr1.Tick -= new EventHandler(Tmr_Tick);   // remove our tick timer
            createHourglass();          // recreate our hourglass
            rotateHourglass();
            simulationStarted = true;
        }

        // pauses the simulator
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if(simulationStarted)
            {
                if (!hourglassDone)
                {
                    if (simulationPaused)
                    {
                        btnPause.Content = "Pause";
                        simulationPaused = false;
                        tmr1.Tick += new EventHandler(Tmr_Tick);
                    }
                    else
                    {
                        btnPause.Content = "Resume";
                        simulationPaused = true;
                        tmr1.Tick -= new EventHandler(Tmr_Tick);
                    }
                }
            }
        }

        // the event for the conclusion of the animation
        private void hourglassrotateanim_Completed(object sender, EventArgs e)
        {
            imgWater.RenderTransform = null;
            invertHourglass();
            //if((!simulationStarted) && (!hourglassDone) && (!simulationPaused))
            //{
            btnPause.Visibility = Visibility.Visible;  // reveal our pause button
            btnReset.Visibility = Visibility.Visible;  // reveal our pause button
            speedcontrolStackPanel.Visibility = Visibility.Visible;
            simulationStarted = true;
            tmr1.Tick += new EventHandler(Tmr_Tick);
            tmr1.Interval = new System.TimeSpan(0, 0, 0, 0, timer_dur);

            // create our background worker thread
            bkw1.DoWork += new System.ComponentModel.DoWorkEventHandler(Iterate);
            bkw1.WorkerSupportsCancellation = true;
            if(!bkw1.IsBusy)
                bkw1.RunWorkerAsync();  // run our background worker
            tmr1.Start();
        }

        // our animation routine
        private void rotateHourglass()
        {
            btnPause.Visibility = Visibility.Collapsed;
            btnStart.Visibility = Visibility.Collapsed;
            btnReset.Visibility = Visibility.Collapsed;
            speedcontrolStackPanel.Visibility = Visibility.Hidden;
            System.Windows.Media.Animation.DoubleAnimation da = new System.Windows.Media.Animation.DoubleAnimation();
            da.From = 0;
            da.To = 180;
            da.Duration = new Duration(TimeSpan.FromSeconds(2));
            da.Completed += new EventHandler(hourglassrotateanim_Completed);

            RotateTransform rt = new RotateTransform();
            imgWater.RenderTransform = rt;
            imgWater.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
        }

        // draw our hourglass in its current configuration
        private void displayHourglass()
        {
            IsDrawingNow = true;
            int r, c;
            Pen bluePen = new Pen(Brushes.Blue, 1);
            Brush blueBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            Pen blkPen = new Pen(Brushes.Black, 1);
            Brush blkBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            double width = cmax * part_size;
            double height = rmax * part_size;
            imgStackpanel.Width = width;
            imgStackpanel.Height = height;

            DrawingVisual vis = new DrawingVisual();
            DrawingContext dc = vis.RenderOpen();
            bmpWater = new RenderTargetBitmap((int)imgWater.Width, (int)imgWater.Height, 96, 96, PixelFormats.Pbgra32);
            //Iterate();
            dc.DrawRectangle(null, bluePen, new Rect(0, 0, width, rmax * height));
            for (r = 0; r < rmax; r++)
            {
                Brush sandBrush;
                if (r >= rmax / 2)
                    sandBrush = new SolidColorBrush(Color.FromRgb((byte)(130 * 2 * r / (3 * rmax)), (byte)(200 * 0.9), 0));
                else
                    sandBrush = new SolidColorBrush(Color.FromRgb((byte)(255 * 2 * r / (3 * rmax)), (byte)(50), 0));

                Pen sandPen = new Pen(sandBrush, 1);

                for (c = 0; c < cmax; c++)
                {
                    if (hourglass[r, c] == 1)
                        dc.DrawEllipse(sandBrush, sandPen, new Point(c * part_size + part_size / 2, r * part_size + part_size / 2), part_size / 2, part_size / 2);
                    if (hourglass[r, c] == 2)
                        dc.DrawEllipse(blkBrush, blkPen, new Point(c * part_size + part_size / 2, r * part_size + part_size / 2), part_size / 2, part_size / 2);
                }
            }
            dc.Close();
            bmpWater.Render(vis);
            imgWater.Source = bmpWater;
            IsDrawingNow = false;
        }

        // invers the hourglass
        private void invertHourglass()
        {
            int[,] temp = new int[rmax, cmax];  // make a temporary hourglass

            //copy the contents of our current hourglass into the temporrary
            for (int r = 0; r<rmax; r++)
            {
                for(int c=0; c<cmax; c++)
                {
                    temp[r, c] = hourglass[r, c];
                }
            }

            //replace the contents of our current hourglass
            for (int r = 0; r < rmax; r++)
            {
                for (int c = 0; c < cmax; c++)
                {
                    hourglass[r, c] = temp[rmax-1-r, cmax-1-c];
                }
            }
        }

        private bool canMove(int r, int c)
        {
            if ((c == 0))                          // if its the leftmost 
            {
                if ((hourglass[r + 1, c] == 0) || (hourglass[r + 1, c + 1] == 0))
                {
                    return true;
                }
            }
            else if (c == cmax - 1)                     // if its the rightmost
            {
                if ((hourglass[r + 1, c] == 0) || (hourglass[r + 1, c - 1] == 0))
                {
                    return true;
                }
            }
            else
            {
                if ((hourglass[r + 1, c] == 0) || (hourglass[r + 1, c - 1] == 0) || (hourglass[r + 1, c + 1] == 0))
                {
                    return true;
                }
            }
            return false;
        }
        // This controls the motion of our sand.
        // This chunk is for a sand type behavior where particles will pile up if they all of the cells below are occupied
        private void Iterate(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            hourglassDone = false;
            int r, c;
            while(!hourglassDone)
            {
                // if the simulation is paused...skip the iteration
                if (simulationPaused)
                    continue;

                hourglassDone = true;
                List<int> randomList = new List<int>();
                List<int> contentList = new List<int>();
                for (r = rmax - 1; r >= 0; r--)
                {
                    randomList.Clear();
                    contentList.Clear();

                    //find eligible particles in a row that can move
                    //          S
                    //        1 2 3
                    //
                    if (r == rmax - 1)
                    {
                        continue;  // if its the bottom row...skip the checks
                    }

                    for (c = 0; c < cmax; c++)
                    {
                        if (hourglass[r, c] != 1)
                        {
                            continue;  // no need to check if our current cell isn't a sand particle
                        }
                        else
                        {
                            // if the space below (2) is open, move immediately (it takes precedence)
                            if(hourglass[r+1, c] == 0)
                            {
                                hourglass[r, c] = 0;
                                hourglass[r + 1, c] = 1;
                                hourglassDone = false;
                                continue;
                            }
                            if(canMove(r, c))
                            {
                                hourglassDone = false;
                                contentList.Add(c); // add the particle to the list 
                            }
                        }
                    }

                    // now shuffle our list 
                    Random ranNum = new Random();
                    int randomIndex = 0;
                    while (contentList.Count > 0)
                    {
                        randomIndex = ranNum.Next(0, contentList.Count);  // choose a random particle from the eligible particles
                        randomList.Add(contentList[randomIndex]); // add it to the new random list
                        contentList.RemoveAt(randomIndex);  // remove entry to avoid duplicates on nextiteration
                    }

                    if (randomList.Count == 0)
                    {
                        continue;
                    }

                    foreach (int item in randomList)
                    {
                        if(canMove(r, item))
                        {
                            hourglassDone = false;
                            if (item == 0)  // leftmost
                            {
                                // check 3 empty
                                if (hourglass[r + 1, item + 1] == 0)
                                {
                                    hourglass[r, item] = 0;
                                    hourglass[r + 1, item + 1] = 1;
                                }
                            }
                            else if (item == cmax - 1)      // rightmost
                            {
                                // check 1 empty
                                if (hourglass[r + 1, item - 1] == 0)
                                {
                                    hourglass[r, item] = 0;
                                    hourglass[r + 1, item - 1] = 1;
                                }
                            } else
                            {
                                // if 1 and 3 are empty
                                if ((hourglass[r+1, item-1] == 0) && (hourglass[r+1, item + 1]==0))
                                {
                                    Random temp = new Random();
                                    if(temp.Next(0,1) ==0)
                                    {
                                        hourglass[r, item] = 0;
                                        hourglass[r + 1, item - 1] = 1;
                                    } else
                                    {
                                        hourglass[r, item] = 0;
                                        hourglass[r + 1, item + 1] = 1;
                                    }

                                }
                                else if (hourglass[r + 1, item - 1] == 0)
                                {
                                    hourglass[r, item] = 0;
                                    hourglass[r + 1, item - 1] = 1;
                                } else if (hourglass[r + 1, item + 1] == 0)
                                {
                                    hourglass[r, item] = 0;
                                    hourglass[r + 1, item + 1] = 1;
                                }
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep(timer_dur);
                this.Dispatcher.Invoke(() =>
                {
                    displayHourglass();
                });

                if (hourglassDone)
                {
                    while (IsDrawingNow)
                    {
                        // Do nothing.
                        // traps the thread until it's done drawing, at which time its safe to break and start the new rotation animation
                    }
                    break;
                }
            }
        }


        //// This controls the motion of our sand and is based on a sample received from Dick Blandford.  In future versions of htis project,
        // this code may be reinstated with a menu option to switch between water and sand mode.
        //// This chunk is for a water type flow where elements can move left or right if all of the cells below are occupied.
        //private void Iterate()
        //{
        //    int r, c;
        //    for(r= rmax-1; r>=0; r--)
        //    {
        //        if(rnd.Next(0,2) == 0)
        //        {
        //            for(c=cmax-1; c >=0; c--)
        //            {
        //                if(hourglass[r,c] == 1)
        //                {
        //                    if (r < rmax-1 && hourglass[r + 1, c] == 0)
        //                    {
        //                        hourglass[r, c] = 0;
        //                        hourglass[r + 1, c] = 1;
        //                    }
        //                    else if ((rnd.Next(0,2) == 1) && (c<cmax-1) && (hourglass[r,c+1] == 0))
        //                    {
        //                        hourglass[r, c] = 0;
        //                        hourglass[r, c + 1] = 1;
        //                    }
        //                    else if((c>0) && (hourglass[r,c-1] == 0))
        //                    {
        //                        hourglass[r, c] = 0;
        //                        hourglass[r, c - 1] = 1;
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            for(c=0; c<cmax; c++)
        //            {
        //                if(hourglass[r,c] == 1)
        //                {
        //                    if (r < rmax-1 && hourglass[r + 1, c] == 0)
        //                    {
        //                        hourglass[r, c] = 0;
        //                        hourglass[r + 1, c] = 1;
        //                    }
        //                    else if ((rnd.Next(0, 2) == 1) && (c < cmax-1) && (hourglass[r, c + 1] == 0))
        //                    {
        //                        hourglass[r, c] = 0;
        //                        hourglass[r, c + 1] = 1;
        //                    }
        //                    else if ((c > 0) && (hourglass[r, c - 1] == 0))
        //                    {
        //                        hourglass[r, c] = 0;
        //                        hourglass[r, c - 1] = 1;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        
        // functionality for our timer.
        private void Tmr_Tick(object sender, EventArgs e)
        {         
            if (hourglassDone)
            {
                tmr1.Tick -= new EventHandler(Tmr_Tick);   // remove our tick timer
                bkw1.CancelAsync();                        // kill our background worker
                bkw1.DoWork -= new System.ComponentModel.DoWorkEventHandler(Iterate);  // remove our background worker
                btnPause.Visibility = Visibility.Collapsed;
                btnReset.Visibility = Visibility.Collapsed;
                rotateHourglass();
            } else
            {
                if(!simulationPaused)
                {
                    displayHourglass();
                } else
                {
                    tmr1.Tick -= new EventHandler(Tmr_Tick);   // remove our tick timer
                }
            }
        }
    }
}
