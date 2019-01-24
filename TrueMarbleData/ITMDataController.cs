using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

/*
Author : Oshan Mendis
ID     : 19222071
Last modified date : 14/03/2018
*/

namespace TrueMarbleData
{
    [ServiceContract]
    public interface ITMDataController
    {
        [OperationContract]
        int GetTileWidth();

        [OperationContract]
        int GetTileHeight();

        [OperationContract]
        int GetNumTilesAcross(int zoom);

        [OperationContract]
        int GetNumTilesDown(int zoom);

        [OperationContract]
        byte[] LoadTiles(int zoom, int x, int y);
    }
}
