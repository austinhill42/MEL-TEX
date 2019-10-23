using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MELTEX.DatabaseControllersOLD
{
    public class DatabaseController : DataContext
    {
        public Table<ItemsTable> Items;
        public Table<InventoryTable> Inventory;

        public DatabaseController(string conn) : base(conn)
        {
           
        }

        //public IList Populate()
        //{
        //    //DatabaseController db = new DatabaseController(App.DBConnString);
        //    //Items = GetTable<ItemsTable>();



        //    return query.ToList();
        //}

    }
}
