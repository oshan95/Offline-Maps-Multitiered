using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

/*
Author : Oshan Mendis
ID     : 19222071
Last modified date : 13/04/2018
*/

namespace TrueMarbleData
{
    //Have set the data tier object to be a singleton by InstanceContextMode attribute
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext =false)]

    //This class contains the implementation of methods which are remotely called by the clients
    internal class TMDataControllerImpl : ITMDataController
    {
        public TMDataControllerImpl()
        {
            System.Console.WriteLine(" New client has connected! ");
        }

        //Requires when the slider value is changed - Tells how many tiles are available at the particular zoom level(horizontally)
        public int GetNumTilesAcross(int zoom)
        {
            int TilesAcross = 0;
            int TilesDown;

            try
            {               
                TMDLLWrapper.GetNumTiles(zoom, out TilesAcross, out TilesDown);
            }
            //If cannot access the dll function
            catch (DllNotFoundException)
            {
                System.Console.WriteLine("Unable to serve client - Could not find the dll");
            }

            return TilesAcross;
        }

        //Requires when the slider value is changed - Tells how many tiles are available at the particular zoom level(vertically)
        public int GetNumTilesDown(int zoom)
        {
            int TilesAcross;
            int TilesDown = 0;

            try
            {
                TMDLLWrapper.GetNumTiles(zoom, out TilesAcross, out TilesDown);
            }
            //If cannot access the dll function
            catch (DllNotFoundException)
            {
                System.Console.WriteLine("Unable to serve client - Could not find the dll");
            }

            return TilesDown;
        }

        //To get the height of the tile selected
        public int GetTileHeight()
        {
            int width;
            int height = 0;

            try
            {
                TMDLLWrapper.GetTileSize(out width, out height);
            }
            //If cannot access the dll function
            catch (DllNotFoundException)
            {
                System.Console.WriteLine("Unable to serve client - Could not find the dll");
            }

            return height;
        }

        //To get the width of the tile selected
        public int GetTileWidth()
        {
            int width = 0;
            int height;

            try
            {
                TMDLLWrapper.GetTileSize(out width, out height);
            }
            //If cannot access the dll function
            catch (DllNotFoundException)
            {
                System.Console.WriteLine("Unable to serve client - Could not find the dll");
            }

            return width;
        }

        //This methods selects the particular tile at a partcular zoom level and coordinates.
        //Then puts the image in a buffer and returns it.
        public byte[] LoadTiles(int zoom, int x, int y)
        {
            int buffSize, imgSize;
            int height = 0;
            int width = 0;
            
            try
            {
                TMDLLWrapper.GetTileSize(out width, out height);
            }
            //If cannot access the dll function
            catch (DllNotFoundException)
            {
                System.Console.WriteLine("Unable to serve client - Could not find the dll");
            }

            //Overestimating buffer size to make sure it's big enough to hold tile
            buffSize = width * height * 3;

            //Creating a buffer to holf raw JPEG data
            byte[] rawJPEG = new byte[buffSize];

            try
            {

                if (TMDLLWrapper.GetTileImageAsRawJPG(zoom, x, y, rawJPEG, buffSize, out imgSize) == 0)
                {
                    System.Console.WriteLine("Possible navigation for tiles out of range detected");
                }
            }
            //If cannot access the dll function
            catch (DllNotFoundException)
            {
                System.Console.WriteLine("Unable to serve client - Could not find the dll");
            }

            return rawJPEG;

        }

        ~TMDataControllerImpl()
        {
            System.Console.WriteLine(" Client Disconnected ");
        }


    }
}
