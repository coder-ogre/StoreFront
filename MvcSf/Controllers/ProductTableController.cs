using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MvcSf.Models;

namespace MvcSf.Controllers
{
    public class ProductTableController : Controller
    {
        private sfdb db = new sfdb();

        // GET: ProductTable
        //public ActionResult Index(string searchString)
        //{
        //    if (String.IsNullOrEmpty(searchString))
        //    {
        //        return View("IndexNullResult");
        //    }
        //    else
        //    {
        //        var productsForSearch = from p in db.ProductTable.Where(s => s.ProductName.Contains(searchString)) select p;
        //        return View(productsForSearch);
        //    }
        //}

        public ActionResult Index(string searchTerm)
        {
            //var resultCount = (from count in db.ProductTable.Where(c => String.IsNullOrEmpty(searchTerm) || c.ProductName.Contains(searchTerm)) select count).Count();
            //var model = db.ProductTable.ToList();
            int cartCount = getCartAmount();
            int ShoppingCartID = getShoppingCartID();
            var products = db.ProductTable.OrderBy(p => p.ProductName).Where(p => String.IsNullOrEmpty(searchTerm) || p.ProductName.Contains(searchTerm))
                .Take(10)
                .Select(p => new ProductTableViewModel
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    ImageFile = p.ImageFile,
                    ProductImage = p.ProductImage,
                    AlreadyInCart = (from productAmount in db.ShoppingCartProductTable.Where(a => p.ProductID == a.ProductID && a.ShoppingCartID == ShoppingCartID) select productAmount.Quantity).FirstOrDefault()?? 0

                    //((from cartCount in db.ShoppingCartProductTable.Where(c => p.ProductID == c.ProductID && c.ShoppingCartID == () select cartCount))
                    //ResultCount = resultCount
                });

            //var products = (from p in db.ProductTable where String.IsNullOrEmpty(searchTerm) || p.ProductName.Contains(searchTerm) orderby p.ProductName select new ProductTableViewModel
            //{
            //    ProductID = p.ProductID,
            //    ProductName = p.ProductName,
            //    ProductDescription = p.ProductDescription,
            //    Quantity = p.Quantity,
            //    Price = p.Price
            //}).ToList();
            return View(products);
        }

        // GET: ProductTable/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductTable productTable = db.ProductTable.Find(id);
            if (productTable == null)
            {
                return HttpNotFound();
            }
            return View(productTable);
        }

        // GET: ProductTable/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductTable/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,ProductName,ProductDescription,IsPublished,Quantity,Price,ImageFile,DateCreated,CreatedBy,DateModified,ModifiedBy,ProductImage")] ProductTable productTable)
        {
            if (ModelState.IsValid)
            {
                db.ProductTable.Add(productTable);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(productTable);
        }

        // GET: ProductTable/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductTable productTable = db.ProductTable.Find(id);
            if (productTable == null)
            {
                return HttpNotFound();
            }
            return View(productTable);
        }

        // POST: ProductTable/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,ProductDescription,IsPublished,Quantity,Price,ImageFile,DateCreated,CreatedBy,DateModified,ModifiedBy,ProductImage")] ProductTable productTable)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productTable).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productTable);
        }

        // GET: ProductTable/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductTable productTable = db.ProductTable.Find(id);
            if (productTable == null)
            {
                return HttpNotFound();
            }
            return View(productTable);
        }

        // POST: ProductTable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductTable productTable = db.ProductTable.Find(id);
            OrderProductTable ocpt = (from orderItem in db.OrderProductTable.Where(oi => productTable.ProductID == oi.ProductID) select orderItem).FirstOrDefault();
            ShoppingCartProductTable scpt = (from cartItem in db.ShoppingCartProductTable.Where(ci => productTable.ProductID == ci.ProductID) select cartItem).FirstOrDefault();
            if(scpt != null)
            {
                db.ShoppingCartProductTable.Remove(scpt);
            }
            if (scpt != null)
            {
                db.OrderProductTable.Remove(ocpt);
            }
            db.ProductTable.Remove(productTable);
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

        public int getCartAmount()
        {
            var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
            int i = 0;
            if (userQuery != null)
            {
                var matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();
                if (matchCartToUserQuery != null)
                {
                    var getCartItemFromCart = (from cartItemLinq in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == matchCartToUserQuery.ShoppingCartID) select cartItemLinq);
                    foreach (var countTheseProducts in (from cartItemLinq in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == matchCartToUserQuery.ShoppingCartID) select cartItemLinq).ToList())
                    {
                        i = i + countTheseProducts.Quantity.Value;
                    }
                }
            }
            return i;
        }

        public int getShoppingCartID()
        {
            int matchIDToUserQuery;
            try
            {
                int userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq.UserID).FirstOrDefault();
                matchIDToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery) select shoppingCartLinq.ShoppingCartID).FirstOrDefault();
            }
            catch
            {
                matchIDToUserQuery = -1;
            }
            return matchIDToUserQuery;
        }
    }
}
