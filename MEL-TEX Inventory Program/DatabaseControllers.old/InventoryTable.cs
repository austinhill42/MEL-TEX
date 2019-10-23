using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Text;
using System.Threading.Tasks;

namespace MELTEX.DatabaseControllersOLD
{
    [Table(Name = "Inventory")]
    public class InventoryTable
    {
        /*********************************
        * Column mappings for Items table 
        *********************************/
        private string _InventoryItem;
        [Column(Name = "Inventory_Item", Storage = "_InventoryItem", DbType = "nvarchar(50) NOT NULL")]
        public string InventoryItem
        {
            get { return this._InventoryItem; }
            set { this._InventoryItem = value; }
        }

        private string _ActualCost;
        [Column(Name = "Actual_Cost", Storage = "_ActualCost", DbType = "decimal(18,2)")]
        public string ActualCost
        {
            get { return this._ActualCost; }
            set { this._ActualCost = value; }
        }

        private string _Cost;
        [Column(Storage = "_Cost", DbType = "decimal(18,2) NOT NULL")]
        public string Cost
        {
            get { return this._Cost; }
            set { this._Cost = value; }
        }

        private string _BarcodeNo;
        [Column(Name = "Barcode_No", Storage = "_BarcodeNo", DbType = "int NOT NULL")]
        public string BarcodeNo
        {
            get { return this._BarcodeNo; }
            set { this._BarcodeNo = value; }
        }

        private string _Warehouse;
        [Column(Storage = "_Warehouse", DbType = "nvarchar(50) NOT NULL")]
        public string Warehouse
        {
            get { return this._Warehouse; }
            set { this._Warehouse = value; }
        }

        private string _BIN;
        [Column(Storage = "_BIN", DbType = "nvarchar(50) NOT NULL")]
        public string BIN
        {
            get { return this._BIN; }
            set { this._BIN = value; }
        }

        private string _Quantity;
        [Column(Storage = "_Quantity", DbType = "int NOT NULL")]
        public string Quantity
        {
            get { return this._Quantity; }
            set { this._Quantity = value; }
        }

        private string _QuantityAvail;
        [Column(Storage = "_QuantityAvail", DbType = "int NOT NULL")]
        public string QuantityAvail
        {
            get { return this._QuantityAvail; }
            set { this._QuantityAvail = value; }
        }

        private string _PO;
        [Column(Name = "PO#", Storage = "_PO", DbType = "nvarchar(50) NOT NULL")]
        public string PO
        {
            get { return this._PO; }
            set { this._PO = value; }
        }

        private string _Notes;
        [Column(Storage = "_Notes", DbType = "nvarchar(MAX) NOT NULL")]
        public string Notes
        {
            get { return this._Notes; }
            set { this._Notes = value; }
        }
    }
}
