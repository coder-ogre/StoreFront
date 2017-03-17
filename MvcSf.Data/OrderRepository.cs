using MvcSf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSf
{
    public class OrderRepository
    {
        sfdb db = new sfdb();

        public MvcSf.Models.OrderTable GetOrder(int id)
        {
            return GetOrders().Where(o => o.OrderID == id).FirstOrDefault();
        }

        public System.Linq.IQueryable<MvcSf.Models.OrderTable> GetOrders()
        {
            var orders = (from p in db.OrderTable select p);
            return orders;
        }
    }
}