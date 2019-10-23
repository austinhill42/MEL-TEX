using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MELTEX.DatabaseControllersOLD
{
    [Table(Name = "Items")]
    public class ItemsTable
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

        private string _Description;
        [Column(Storage = "_Description", DbType = "nvarchar(MAX) NOT NULL")]
        public string Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }

        private string _Weight;
        [Column(Storage = "_Weight", DbType = "decimal(18,2)")]
        public string Weight
        {
            get { return this._Weight; }
            set { this._Weight = value; }
        }

        private string _ListPrice;
        [Column(Storage = "_ListPrice", DbType = "decimal(18,2) NOT NULL")]
        public string List_Price
        {
            get { return this._ListPrice; }
            set { this._ListPrice = value; }
        }

        private string _Group;
        [Column(Storage = "_Group", DbType = "nvarchar(MAX) NOT NULL")]
        public string Group
        {
            get { return this._Group; }
            set { this._Group = value; }
        }

        private string _Multiplier;
        [Column(Storage = "_Multiplier", DbType = "decimal(18,2) NOT NULL")]
        public string Multiplier
        {
            get { return this._Multiplier; }
            set { this._Multiplier = value; }
        }

        private string _PublishedSales;
        [Column(Name = "Published_Sales", Storage = "_PublishedSales", DbType = "decimal(18,2) NOT NULL")]
        public string PublishedSales
        {
            get { return this._PublishedSales; }
            set { this._PublishedSales = value; }
        }

        private string _PublishedCost;
        [Column(Name = "Published_Cost", Storage = "_PublishedCost", DbType = "decimal(18,2) NOT NULL")]
        public string PublishedCost
        {
            get { return this._PublishedCost; }
            set { this._PublishedCost = value; }
        }

        private string _Notes;
        [Column(Storage = "_Notes", DbType = "nvarchar(MAX)")]
        public string Notes
        {
            get { return this._Notes; }
            set { this._Notes = value; }
        }
    }
}
