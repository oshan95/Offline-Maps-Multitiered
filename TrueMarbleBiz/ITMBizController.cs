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

namespace TrueMarbleBiz
{

    [ServiceContract]
    public interface ITMBizControllerCallBack
    {
        [OperationContract]
        void OnVerificationComplete(bool res);
    }

    //Remote Procedure Calls from client are directed to here
    [ServiceContract(CallbackContract = typeof(ITMBizControllerCallBack))]
    public interface ITMBizController
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

        [OperationContract]
        bool VerifyTiles();

        [OperationContract]
        void VerifyTilesAsync();

        [OperationContract]
        void AddHistEntry(int x, int y, int zoom);

        [OperationContract]
        HistEntry GetCurrHistEntry();

        [OperationContract]
        HistEntry HistBack();

        [OperationContract]
        HistEntry HistForward();

        [OperationContract]
        BrowseHistory GetFullHistory();

        [OperationContract]
        void SetFullHistory(BrowseHistory history);
    }

}
