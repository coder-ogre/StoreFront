using MvcSf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSf
{
    public class InventoryRepository
    {
        sfdb db = new sfdb();
        public System.Collections.Generic.List<MvcSf.Models.ProductTable> SearchProducts(string searchText)
        {
            //var productsForSearch = from p in db.ProductTables select p;
            var productsForSearch = GetProducts().Where(s => s.ProductName.Contains(searchText));
            if (productsForSearch.Equals(null))
            {
                productsForSearch = productsForSearch.Where(s => s.ProductDescription.Contains(searchText));
            }
            return productsForSearch.ToList();
        }

        public MvcSf.Models.ProductTable GetProduct(int id)
        {
            return GetProducts().Where(p => p.ProductID == id).FirstOrDefault();
        }

        public System.Linq.IQueryable<MvcSf.Models.ProductTable> GetProducts()
        {
            var products = from p in db.ProductTable select p;
            return products;
        }
    }
}