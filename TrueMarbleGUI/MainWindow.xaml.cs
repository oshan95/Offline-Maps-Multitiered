using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
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
using TrueMarbleBiz;

/*
Author : Oshan Mendis
ID     : 19222071
Last modified date : 13/04/2018
Reference : https://msdn.microsoft.com/en-us/library/system.windows.forms.filedialog.filter(v=vs.110).aspx
*/

namespace TrueMarbleGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ITMBizControllerCallBack
    {
        ITMBizController m_biz;

        //These variables are for the use of recording history in disk
        OpenFileDialog fileOpenWindow;
        SaveFileDialog fileSaveWindow;
        Stream myStream, myStream2;

        //For use of foward & back buttons 
        HistEntry history;

        int ZoomValue;
        int acrossX = 0;
        int downY = 0;
        //For the use of slider
        private bool LoadTileClicked = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        //This method contains the coding need to establish a connection with the biz tier when the programme is running
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
                NetTcpBinding ntb = new NetTcpBinding();

                //To relax the maximum size of a message in .net(Since images are being passed arround)
                ntb.MaxReceivedMessageSize = System.Int32.MaxValue;
                ntb.ReaderQuotas.MaxArrayLength = System.Int32.MaxValue;
            try
            {
                DuplexChannelFactory<ITMBizController> TMFactory = new DuplexChannelFactory<ITMBizController>(this, ntb, "net.tcp://localhost:50002/TMBiz");

                m_biz = TMFactory.CreateChannel();
                m_biz.VerifyTilesAsync();
                
            }

            //If URI is invalid or uri Cannot be parsed or empty uri
            catch (UriFormatException ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
            //If endpoint is null
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }

            //If address is null
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }

            //If address is null
            catch (EndpointNotFoundException)
            {
                MessageBox.Show("Error : Coulld not connect with the server. Please try again later!");
            }
        }

        //This method is executed when the "load tile" button is clicked
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadTileClicked = true;
            ZoomValue = 0;
            LoadTile(true);
        }

        //Coding needs to load the corresponding tile to the picture frame of program window
        public void LoadTile(bool addToHist)
        {
            try
            {
                int zoom = ZoomValue;
                int x = acrossX;
                int y = downY;
                byte[] rawImg = m_biz.LoadTiles(zoom, x, y);
                MemoryStream ms = new MemoryStream(rawImg);
                JpegBitmapDecoder jbd = new JpegBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.None);
                imgTile.Source = jbd.Frames[0];
                //if (rawImg == null && rawImg.Length > 0)
                //{
                //    System.Console.WriteLine("No image found for the coordinates!");
                //}

                //Makes sure back & forward moves in history doesn't get added to the list 
                if (addToHist == true)
                {
                    m_biz.AddHistEntry(x, y, zoom);
                }

            }

            catch (NotSupportedException ex)
            {
                System.Console.WriteLine("Error : " + ex.Message);

            }

            //If cannot reach the server(data tier)
            catch (EndpointNotFoundException ex)
            {
                System.Console.WriteLine("Error : " + ex.Message);
            }

            //If the connection with server is lost/ Connection to the server have not built yet
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show("Error : Connection with the server is lost ");
            }

            //If the connection with server is lost/ Connection to the server have not built yet
            catch (CommunicationException)
            {
                MessageBox.Show("Error : Connection with the server is lost ");
            }

            /*If the returned buffer from the biz tier is null
            Two reasons for buffer to be null
                1) Data tier has exited implicitly
                2) Data tier program have not been run yet
            */
            catch (ArgumentNullException)
            {
                MessageBox.Show("Cannot retrieve data");
            }

            catch (NullReferenceException)
            {
                MessageBox.Show("Could not verify tiles!");
            }
        }

        //To load the corresponding tile when the button to right of the picture frame is clicked
        private void btnEast_Click(object sender, RoutedEventArgs e)
        {
            acrossX++;
            
            try
            {
                //Checking Boundaries
                if (acrossX < 0 || acrossX == 0)
                {
                    acrossX = 0;
                    LoadTile(true);
                }
                //Making sure the request for the tile is withing number of tiles
                else if (acrossX < m_biz.GetNumTilesAcross(ZoomValue))
                {
                    LoadTile(true);
                }
                else
                {
                    acrossX = m_biz.GetNumTilesAcross(ZoomValue) - 1;
                    LoadTile(true);
                }
            }

            //If the connection with server is lost
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show("Error : Connection with the server is lost ");
            }

            //If the connection with server is lost
            catch (CommunicationException)
            {
                MessageBox.Show("Error : Connection with the server is lost ");
            }
        }

        //To load the corresponding tile when the button to down of the picture frame is clicked
        private void btnSouth_Click(object sender, RoutedEventArgs e)
        {
            downY++;
            
            try
            {
                //Checking Boundaries
                if (downY < 0 || downY == 0)
                {
                    downY = 0;
                    LoadTile(true);
                }
                //Making sure the request for the tile is withing number of tiles
                else if (downY < m_biz.GetNumTilesDown(ZoomValue))
                {
                    LoadTile(true);
                }
                else
                {
                    downY = m_biz.GetNumTilesDown(ZoomValue) - 1;
                    LoadTile(true);
                }
            }

            //If the connection with server is lost
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show("Error : Connection with the server is lost ");
            }

            //If the connection with server is lost
            catch (CommunicationException)
            {
                MessageBox.Show("Error : Connection with the server is lost ");
            }

        }

        //To load the corresponding tile when the button to left of the picture frame is clicked
        private void btnWest_Click(object sender, RoutedEventArgs e)
        {
            acrossX--;
            
            try
            {
                //Checking Boundaries
                if (acrossX < 0 || acrossX == 0)
                {
                    acrossX = 0;
                    LoadTile(true);
                }
                //Making sure the request for the tile is withing number of tiles
                else if (acrossX < m_biz.GetNumTilesAcross(ZoomValue))
                {
                    LoadTile(true);
                }
                else
                {
                    acrossX = m_biz.GetNumTilesAcross(ZoomValue) - 1;
                    LoadTile(true);
                }
            }

            //If the connection with server is lost
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show("Error : Connection with the server is lost ");
            }

            //If the connection with server is lost
            catch (CommunicationException)
            {
                MessageBox.Show("Error : Connection with the server is lost ");
            }

        }

        //To load the corresponding tile when the button to top of the picture frame is clicked
        private void btnNorth_Click(object sender, RoutedEventArgs e)
        {
            downY--;
            
            try
            {
                //Checking Boundaries
                if (downY < 0 || downY == 0)
                {
                    downY = 0;
                    LoadTile(true);
                }
                //Making sure the request for the tile is withing number of tiles
                else if (downY < m_biz.GetNumTilesDown(ZoomValue))
                {
                    LoadTile(true);
                }
                else
                {
                    downY = m_biz.GetNumTilesDown(ZoomValue) - 1;
                    LoadTile(true);
                }
            }

            //If the connection with server is lost
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show("Error : Connection with the server is lost ");
            }

            //If the connection with server is lost
            catch (CommunicationException)
            {
                MessageBox.Show("Error : Connection with the server is lost ");
            }
        }

        //Coding requires to load the corresponding tile when the slider value is changed
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LoadTileClicked == true)
            {
                ZoomValue = (int)slider.Value;
                LoadTile(true);
            }
            else
            {
                MessageBox.Show("Please click load tile before using slider!");
                slider.Value = 0;
            }
        }

        //Tiles verification
        public void OnVerificationComplete(bool res)
        {
            if (res)
            {
                MessageBox.Show("All the tiles were verified successfully");
            }
            else
            {
                MessageBox.Show("Couldn't verify some of the tiles");
            }
        }

        //Back button in the gui to view the history
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                history = m_biz.HistBack();

                //Getting the coordinates from the history
                ZoomValue = history.zoom;
                acrossX = history.x;
                downY = history.y;

                //Loads the tile but doesn't record in the history
                LoadTile(false);
            }
            //If server is not running
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show("Error: Cannot connect to the server. Please try again later!");
            }

            //If user haven't navigate yet to record history anf then tries to view history
            catch (NullReferenceException)
            {
                MessageBox.Show("Error: Please start navigating to record the history!");
            }
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                history = m_biz.HistForward();

                //Getting the coordinates from the history
                ZoomValue = history.zoom;
                acrossX = history.x;
                downY = history.y;

                //Loads the tile but doesn't record in the history
                LoadTile(false);
            }

            //If server is not running
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show("Error: Cannot connect to the server. Please try again later!");
            }

            //If user haven't navigate yet to record history anf then tries to view history
            catch (NullReferenceException)
            {
                MessageBox.Show("Error: Please start navigating to record the history!");
            }
        }

        //This method is used to save navigation history on a disk 
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(LoadTileClicked==true)
            {
                BrowseHistory bh = null;

                try
                {
                    bh = m_biz.GetFullHistory();
                }
                catch(CommunicationObjectFaultedException)
                {
                    MessageBox.Show("Error: Connection With the server is lost");
                }

                //Have used save file dialog window to save the file
                //Save file dialog window is utilized to save in only .xml extensions
                fileSaveWindow = new SaveFileDialog();
                fileSaveWindow.InitialDirectory = "c:\\";
                fileSaveWindow.Filter = "XML files (*.xml)|*.xml";
                fileSaveWindow.RestoreDirectory = true;

                //If the save dialog opens up 
                if (fileSaveWindow.ShowDialog() == true && bh != null)
                {
                    try
                    {
                        //In here checks if the stream is null(Checks if the file is empty to load)
                        if ((myStream2 = fileSaveWindow.OpenFile()) != null)
                        {
                            //Serializing browse history to save it in the xml
                            DataContractSerializer serializer = new DataContractSerializer(typeof(BrowseHistory));
                            serializer.WriteObject(myStream2, bh);
                            myStream2.Close();
                        }
                    }
                    //If the file name is empty
                    catch (ArgumentNullException)
                    {
                        MessageBox.Show("Error: File name is empty!");
                    }

                    //serializer encountering invalid data
                    catch (InvalidDataContractException)
                    {
                        MessageBox.Show("Error: File name is empty!");
                    }

                    //Error in serialization/De serialization
                    catch (SerializationException)
                    {
                        MessageBox.Show("Error: File name is empty!");
                    }

                    //Message exceeding quota
                    catch (QuotaExceededException)
                    {
                        MessageBox.Show("Error: Cannot write more data to disk!");
                    }

                    //If biz tier/data tier program is not up and running yet
                    catch (CommunicationObjectFaultedException)
                    {
                        MessageBox.Show("Error: The server is not responding. Please try again later!");
                    }
                }
            }
            else
            {
                MessageBox.Show("First load tile and navigate through tiles to collect history");
            }
            

        }

        //New form window is opened to display history
        //ShowDialog() method blocks accessing main window until the new dialog window is closed
        private void btnViewHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DisplayHistory History = new DisplayHistory(m_biz.GetFullHistory());
                History.ShowDialog();
            }

            //If Connection with the biz tier is lost
            catch(CommunicationObjectFaultedException)
            {
                MessageBox.Show("Error: Lost the connection with the server");
            }

            //Errors in forms
            catch (InvalidOperationException)
            {
                MessageBox.Show("Error: The form is already bieng shown");
            }
        }

        //This method is used to load navigation history from the disk
        private void btnLoadHist_Click(object sender, RoutedEventArgs e)
        {
            BrowseHistory bh;

            //Have used open file dialog window to open the file
            //Save file dialog window is utilized to open only .xml files
            fileOpenWindow = new OpenFileDialog();
            fileOpenWindow.InitialDirectory = "c:\\";
            fileOpenWindow.Filter = "XML files (*.xml)|*.xml";
            fileOpenWindow.RestoreDirectory = true;

            //checks if the open file dialog opens up 
            if (fileOpenWindow.ShowDialog() == true)
            {
                try
                {   
                    //Loading the file to the stream
                    if ((myStream = fileOpenWindow.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            //Getting ready the serializer for dezerialization 
                            DataContractSerializer serializer = new DataContractSerializer(typeof(BrowseHistory));
                            bh = (BrowseHistory)serializer.ReadObject(myStream);
                            m_biz.SetFullHistory(bh);
                            myStream.Close();
                        }
                    }
                }
                //If the file name is empty
                catch (ArgumentNullException)
                {
                    MessageBox.Show("Error: File name is empty!");
                }

                //If biz tier/data tier program is not up and running yet
                catch (CommunicationObjectFaultedException)
                {
                    MessageBox.Show("Error: The server is not responding. Please try again later!");
                }

                //serializer encountering invalid data
                catch (InvalidDataContractException)
                {
                    MessageBox.Show("Error: File name is empty!");
                }

                //Error in serialization/De serialization
                catch (SerializationException)
                {
                    MessageBox.Show("Error: File name is empty!");
                }

                //Message exceeding quota
                catch (QuotaExceededException)
                {
                    MessageBox.Show("Error: Cannot write more data to disk!");
                }
            }
        }
    }
}
