using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TrueMarbleData;

/*
Author : Oshan Mendis
ID     : 19222071
Last modified date : 13/04/2018
*/

namespace TrueMarbleBiz
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]

    //This class contains implementation of methods that are defined in the ITMBizController (Business logic)
    internal class TMBizControllerImpl : ITMBizController
    {
        ITMDataController m_data;
        BrowseHistory m_hist;

        //This will enable biz tier to provide service as server to the gui clients and be a client to the data tier
        public TMBizControllerImpl()
        {
            NetTcpBinding ntb = new NetTcpBinding();

            //To relax the maximum size of a message in .net(Since images are being passed arround)
            ntb.MaxReceivedMessageSize = System.Int32.MaxValue;
            ntb.ReaderQuotas.MaxArrayLength = System.Int32.MaxValue;
            try
            {
                ChannelFactory<ITMDataController> TMFactory = new ChannelFactory<ITMDataController>(ntb, "net.tcp://localhost:50001/TMData");

                m_data = TMFactory.CreateChannel();

                //Creating a new browse history object
                m_hist= new BrowseHistory();

                System.Console.WriteLine("New GUI Client Connected!");
            }

            //If URI is invalid or uri Cannot be parsed or empty uri
            catch (UriFormatException)
            {
                Console.WriteLine("Error : The URI is invalid or corrupted");
            }
            //If endpoint is null
            catch (InvalidOperationException)
            {
                Console.WriteLine("Error : The method cannot be executed according to the object's current state ");
            }

            //If address is null
            catch (ArgumentNullException)
            {
                Console.WriteLine("Error : Address of data tier could'nt be found");
            }
        }

        ~TMBizControllerImpl()
        {
            System.Console.WriteLine("The Client disconnected!");
        }

        // 1) Calling the functions at data tier. And getting the return value of data tier's function
        // 2) Call goes to the ITMDataController(Interface)
        // 3) From the interface the call is diverted to TMDataControllerImpl class 
        // 4) This ensures gui clients does not have direct access to the data tier
        public int GetNumTilesAcross(int zoom)
        {
            int ret = 0; 

            try
            {
                ret = m_data.GetNumTilesAcross(zoom);
            }
            catch(CommunicationException)
            {
                System.Console.WriteLine("Error : The data tier program exited implicitly");
            }

            return ret;
        }

        // 1) Calling the functions at data tier. And getting the return value of data tier's function
        // 2) Call goes to the ITMDataController(Interface)
        // 3) From the interface the call is diverted to TMDataControllerImpl class 
        // 4) This ensures gui clients does not have direct access to the data tier
        public int GetNumTilesDown(int zoom)
        {
            int ret = 0;

            try
            {
                ret = m_data.GetNumTilesDown(zoom);
            }
            catch (CommunicationException)
            {
                System.Console.WriteLine("Error : The data tier program exited implicitly");
            }

            return ret;
        }

        // 1) Calling the functions at data tier. And getting the return value of data tier's function
        // 2) Call goes to the ITMDataController(Interface)
        // 3) From the interface the call is diverted to TMDataControllerImpl class 
        // 4) This ensures gui clients does not have direct access to the data tier
        public int GetTileHeight()
        {
            int ret = 0;

            try
            {
                ret = m_data.GetTileHeight();
            }
            catch (CommunicationException)
            {
                System.Console.WriteLine("Error : Data tier program is not running!");
            }

            return ret;
        }

        // 1) Calling the functions at data tier. And getting the return value of data tier's function
        // 2) Call goes to the ITMDataController(Interface)
        // 3) From the interface the call is diverted to TMDataControllerImpl class 
        // 4) This ensures gui clients does not have direct access to the data tier
        public int GetTileWidth()
        {
            int ret = 0;

            try
            {
                ret = m_data.GetTileWidth();
            }
            catch (CommunicationException)
            {
                System.Console.WriteLine("Error : Data tier program is not running!");
            }

            return ret;
        }

        // 1) Calling the functions at data tier. And getting the return value of data tier's function
        // 2) Call goes to the ITMDataController(Interface)
        // 3) From the interface the call is diverted to TMDataControllerImpl class 
        // 4) This ensures gui clients does not have direct access to the data tier
        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] LoadTiles(int zoom, int x, int y)
        {

            byte[] ret = null;

            try
            {
                ret = m_data.LoadTiles(zoom, x, y);
            }

            //If data tier program exited after it's execution
            catch (CommunicationObjectFaultedException)
            {
                System.Console.WriteLine("Error : Data tier program is not running!");
            }

            //If data tier program have not already started
            catch (EndpointNotFoundException)
            {
                System.Console.WriteLine("Error : Data tier program have not started already");
            }

            return ret;
        }

        //This method ensure all the tiles are available and returns false if any of the tiles are missing.
        //Have added reference to the presentation core to use the JpegBitmapDecoder
        public bool VerifyTiles()
        {
            //This bool variable won't be changed unless an exception is captured
            bool OperationSuccess = true;

            //Making sure coordinates are initially wrong
            int zoom = -1;
            int x = -1;
            int y = -1;

            try
            {
                for (zoom = 0; zoom < 7; zoom++)
                {
                    int TilesAcross = GetNumTilesAcross(zoom);
                    int TilesDown = GetNumTilesDown(zoom);

                    //X coordinate of the tile that is going to be checked
                    for (x = 0; x < TilesAcross; x++)
                    {
                        //y coordinate of the tile that is going to be checked
                        for (y = 0; y < TilesDown; y++)
                        {
                            byte[] rawImg = m_data.LoadTiles(zoom, x, y);
                            MemoryStream ms = new MemoryStream(rawImg);
                            JpegBitmapDecoder decoder = new JpegBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.None);
                        }
                    }
                }
            }

            //For Incorrcet zoom value and coordinates
            catch (NotSupportedException)
            {
                System.Console.WriteLine("Could not verify the tile at zoom level of " + zoom + ", " + "X coordinate of " + x + ", Y coordinate of " + y);
                OperationSuccess = false;
            }

            return OperationSuccess;

        }

        //This delegate is required to make the asynchronous call
        private delegate bool VerifyTilesDelegate();

        //This method is used to call VerifyTiles() asynchronously
        public void VerifyTilesAsync()
        {
            //Assigning the function VerifyTiles() to the delegate
            VerifyTilesDelegate TilesDel = VerifyTiles;

            AsyncCallback cb = this.VerifyTiles_OnComplete;            
            TilesDel.BeginInvoke(cb, OperationContext.Current.GetCallbackChannel<ITMBizControllerCallBack>());
            System.Console.WriteLine("Waiting for the completion of verification");
        }

        //VerfyTilesAsync() method will call this function for completeion callback(Completion callback is executed from this method)
        public void VerifyTiles_OnComplete(IAsyncResult res)
        {
            //To check if EndInvoke is executed successfully(return of VerifyTiles() method)
            bool success;

            ITMBizControllerCallBack BizClBck;
            VerifyTilesDelegate TilesDel;
            AsyncResult asyncObj = (AsyncResult)res;

            if (asyncObj.EndInvokeCalled == false)
            {
                TilesDel = (VerifyTilesDelegate)asyncObj.AsyncDelegate;
                success = TilesDel.EndInvoke(asyncObj);

                //casting return value of asyncObj.AsyncState to type of ITMBizController call back
                BizClBck = (ITMBizControllerCallBack)asyncObj.AsyncState;

                BizClBck.OnVerificationComplete(success);
                asyncObj.AsyncWaitHandle.Close();
            }
        }

        //Adds history to the list when user navigates through tiles
        public void AddHistEntry(int x, int y, int zoom)
        {
            
            try
            {
                HistEntry he = new HistEntry(x, y, zoom);
                m_hist.AddHistEntry(he);
            }
            catch(NullReferenceException)
            {
                System.Console.WriteLine("Attempt for saving history just after loadin tile!");
            }

        }

        //To get teh most previously added history
        public HistEntry GetCurrHistEntry()
        {
            return m_hist.GetCurrHistEntry();
        }

        //Method that will be fired when the user clicks the back history button
        public HistEntry HistBack()
        {
            try
            {
                m_hist.HistBack();
            }
            catch(NullReferenceException)
            {
                System.Console.WriteLine("History object is empty");
            }

            return m_hist.GetCurrHistEntry();

        }

        //Method that will be fired when the user clicks the forward history button
        public HistEntry HistForward()
        {
            m_hist.HistForward();
            return m_hist.GetCurrHistEntry();
        }

        //Load a browse history from the disk
        public BrowseHistory GetFullHistory()
        {
            return m_hist;
        }

        //Save history to a disk
        public void SetFullHistory(BrowseHistory history)
        {
            m_hist = history;
        }
    }
}
