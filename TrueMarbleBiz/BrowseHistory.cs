using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
    //To track user's current location in the history
    [DataContract]
    public class HistEntry
    {
        [DataMember]
        public int x { get; set; }

        [DataMember]
        public int y { get; set; }

        [DataMember]
        public int zoom { get; set; }

        public HistEntry(int ValX, int ValY, int ValZoom)
        {
            this.x = ValX;
            this.y = ValY;
            this.zoom = ValZoom;
        }
    }

    //This class helps to keep the history of tiles that already visited
    [DataContract]
    public class BrowseHistory
    {
        [DataMember]
        public List<HistEntry> History { get; set; }

        [DataMember]
        public int CurrEntryIdx { get; set; }

        //Default Constructor
        public BrowseHistory()
        {
            //Empty list - Before user navigates through tiles
            this.History = new List<HistEntry>();

            //Shows that user haven't navigate through tiles yet
            this.CurrEntryIdx = -1;
        }

        //This method will be called everytime user navigates through tile(Not when viewing history)
        [OperationContract]
        public void AddHistEntry(HistEntry hist)
        {
            if (CurrEntryIdx < History.Count - 1)
            {
                //Removing previous history entries when user calls HistBack() Several times and then add a new entry
                //CurrEntryIndex + 1 - is becuase we need to save only the current entry as the history left when user navigates through tiles again
                this.History.RemoveRange(CurrEntryIdx + 1, History.Count - (CurrEntryIdx + 1));
            }
            this.History.Add(hist);

            //Helps to get the next available index of the list
            CurrEntryIdx++;
        }

        //Gets the most previously added index of the list
        [OperationContract]
        public HistEntry GetCurrHistEntry()
        {
            HistEntry x = null ;

            try
            {
                x = History[CurrEntryIdx];
            }
            //If Load tile button have not clicked already
            catch (ArgumentOutOfRangeException)
            {
                System.Console.WriteLine();
            }

            return x;

        }

        //Sets the current location of the list when user views the history(Location of the history currently viewing)
        [OperationContract]
        public void HistBack()
        {
            //If condtion makes sure their are more backward history
            if (CurrEntryIdx > 0)
            {
                CurrEntryIdx--;
            }
        }

        //Sets the current location of the list when user views the history(Location of the history currently viewing)
        [OperationContract]
        public void HistForward()
        {
            //If condtion makes sure their are more history to forward
            if (CurrEntryIdx < History.Count - 1)
            {
                CurrEntryIdx++;
            }
        }
    }
}
