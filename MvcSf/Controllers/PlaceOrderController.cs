using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MvcSf.Models;
using MvcSf.ViewModels;

namespace MvcSf.Controllers
{
    public class PlaceOrderController : Controller
    {
        private sfdb db = new sfdb();

        // GET: PlaceOrder
        public ActionResult Index()
        {
            var orderTable = db.OrderTable.Include(o => o.AddressTable).Include(o => o.StatusTable).Include(o => o.UserTable);
            return View(orderTable.ToList());
        }

        // GET: PlaceOrder/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderTable orderTable = db.OrderTable.Find(id);
            if (orderTable == null)
            {
                return HttpNotFound();
            }
            return View(orderTable);
        }

        public ActionResult CreateAddress()
        {
            return View();
        }

        // GET: PlaceOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAddress(int id, [Bind(Include = "Address1, Address2, Address3, City, StateID, ZipCode, IsBilling, IsShipping, DateCreated, CreatedBy, DateModified, ModifiedBy")] AddressTable addressTable)
        {
            ViewBag.AddressID = new SelectList(db.AddressTable, "AddressID", "Address1");
            ViewBag.StatusID = new SelectList(db.StatusTable, "StatusID", "StatusDescription");
            ViewBag.UserID = new SelectList(db.UserTable, "UserID", "UserName");


            if (ModelState.IsValid)
            {
                addressTable.UserID = id;
                db.AddressTable.Add(addressTable);
                db.SaveChanges();
                return RedirectToAction("ChooseAddress");
            }

            return View(addressTable);
        }

        public ActionResult ChooseAddress()
        {
            //var addressTable = db.AddressTable.Include(which => which.OrderTable).Include(which => which.UserTable);
            var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
            ViewBag.UserID = userQuery.UserID;
            var addressQuery = from addressLinq in db.AddressTable.Where(addressLinq => addressLinq.UserID == userQuery.UserID) select addressLinq;
            return View(addressQuery.ToList());
        }

        public ActionResult ConfirmOrder(int id)
        {
            var addressQuery = from addressLinq in db.AddressTable.Where(addressLinq => addressLinq.AddressID == id) select addressLinq;
            ViewBag.AddressID = id;
            return View(addressQuery.FirstOrDefault());
        }

        public ActionResult CompleteOrder(int id)
        {
            var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
            var matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();
            var matchCartItemToCart = (from cartItemLinq in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == matchCartToUserQuery.ShoppingCartID) select cartItemLinq);

            OrderTable ot = new OrderTable
            {
                UserID = userQuery.UserID,
                AddressID = id,
                OrderDate = DateTime.Now,
                Total = ViewBag.Price
            };
            db.OrderTable.Add(ot);



            foreach (var item in matchCartItemToCart)
            {

                OrderProductTable opt = new OrderProductTable
                {
                    OrderID = ot.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    DateCreated = DateTime.Now
                };
                db.OrderProductTable.Add(opt);

                var productTemp = (from qualityQualifier in db.ProductTable.Where(QQ => QQ.ProductID == item.ProductID) select qualityQualifier).FirstOrDefault();
                productTemp.Quantity = productTemp.Quantity - item.Quantity;
                if (productTemp.Quantity < 1)
                {
                    db.ProductTable.Remove(productTemp);
                }
                db.ShoppingCartProductTable.Remove(item);
            }

            db.ShoppingCartTable.Remove(matchCartToUserQuery);

            db.SaveChanges();
            return RedirectToAction("Thanks", "PlaceOrder");
        }

        public ActionResult Thanks()
        {
            return View();
        }

        // POST: PlaceOrder/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderID,UserID,AddressID,OrderDate,Total,StatusID,DateCreated,CreatedBy,DateModified,ModifiedBy")] OrderTable orderTable)
        {
            if (ModelState.IsValid)
            {
                db.OrderTable.Add(orderTable);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AddressID = new SelectList(db.AddressTable, "AddressID", "Address1", orderTable.AddressID);
            ViewBag.StatusID = new SelectList(db.StatusTable, "StatusID", "StatusDescription", orderTable.StatusID);
            ViewBag.UserID = new SelectList(db.UserTable, "UserID", "UserName", orderTable.UserID);
            return View(orderTable);
        }

        // GET: PlaceOrder/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderTable orderTable = db.OrderTable.Find(id);
            if (orderTable == null)
            {
                return HttpNotFound();
            }
            ViewBag.AddressID = new SelectList(db.AddressTable, "AddressID", "Address1", orderTable.AddressID);
            ViewBag.StatusID = new SelectList(db.StatusTable, "StatusID", "StatusDescription", orderTable.StatusID);
            ViewBag.UserID = new SelectList(db.UserTable, "UserID", "UserName", orderTable.UserID);
            return View(orderTable);
        }

        // POST: PlaceOrder/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderID,UserID,AddressID,OrderDate,Total,StatusID,DateCreated,CreatedBy,DateModified,ModifiedBy")] OrderTable orderTable)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderTable).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AddressID = new SelectList(db.AddressTable, "AddressID", "Address1", orderTable.AddressID);
            ViewBag.StatusID = new SelectList(db.StatusTable, "StatusID", "StatusDescription", orderTable.StatusID);
            ViewBag.UserID = new SelectList(db.UserTable, "UserID", "UserName", orderTable.UserID);
            return View(orderTable);
        }

        // GET: PlaceOrder/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderTable orderTable = db.OrderTable.Find(id);
            if (orderTable == null)
            {
                return HttpNotFound();
            }
            return View(orderTable);
        }

        // POST: PlaceOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderTable orderTable = db.OrderTable.Find(id);
            db.OrderTable.Remove(orderTable);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
