using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MvcSf.Models;
using System.Web.Services;
using System.Data.SqlClient;
using System.Configuration;

namespace MvcSf.Controllers
{
    public class ShoppingCartProductTableController : Controller
    {
        private sfdb db = new sfdb();

        // GET: ShoppingCartProductTable
        public ActionResult Index()
        {
            //var shoppingCartProductTable = db.ShoppingCartProductTable.Include(s => s.ProductTable).Include(s => s.ShoppingCartTable);
            //return View(shoppingCartProductTable.ToList());
            Console.WriteLine("Welcome to the Shopping Cart.");
            var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
            if (userQuery != null)
            {
                var matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();
                if (matchCartToUserQuery != null)
                {
                    if (ViewBag.cartCount != 0)
                    {
                        ViewBag.Price = getPrice();
                        return View(getCartItems());
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "zzzzNothing is in the cart.  You must add something to the cart from the search products page.";
                        return View("~/Views/Shared/Error.cshtml");
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "zzzzThere is no cart.  Search for an item, and then click on add to cart, to get a cart.";
                    return View("~/Views/Shared/Error.cshtml");

                }
            }
            else
            {
                ViewBag.ErrorMessage = "zzzzYou are not logged in.";
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        public ActionResult AddToCart(int id)
        {
            var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
            if (userQuery == null)
            {
                ViewBag.ErrorMessage = "You need to log in to add an item to your cart";
                //return View("~/Views/Shared/Error.cshtml");
                return Json(new List<int> { 0, 0 });
            }
            ShoppingCartTable matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();

            if (matchCartToUserQuery == null) // adds a shopping cart if needed
            {
                var newShoppingCart = new ShoppingCartTable
                {
                    UserID = userQuery.UserID
                };
                ViewBag.ShoppingCartID = newShoppingCart.ShoppingCartID;
                db.ShoppingCartTable.Add(newShoppingCart);
                db.SaveChanges();
                matchCartToUserQuery = newShoppingCart;
            }

            var getPriceQuery = from priceLinq in db.ProductTable where priceLinq.ProductID == id select priceLinq.Price;
            var getImageQuery = from imageLinq in db.ProductTable where imageLinq.ProductID == id select imageLinq.ImageFile;
            var allQuantities = (from cartItemLinq in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == matchCartToUserQuery.ShoppingCartID) select cartItemLinq.Quantity).ToList();
            int totalQuantity = 0;
            for(int zik = 0; zik < allQuantities.Count(); zik++)
            {
                totalQuantity += (int)((allQuantities[zik] == null)? 0: allQuantities[zik]);
            }
            var matchCartItemToCart = (from cartItemLinq in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == matchCartToUserQuery.ShoppingCartID && c.ProductID == id) select cartItemLinq).FirstOrDefault();
            var productTemp = (from qualityQualifier in db.ProductTable.Where(QQ => QQ.ProductID == id) select qualityQualifier).FirstOrDefault();
            if (!(matchCartItemToCart == null))
            {
                matchCartItemToCart.Quantity = matchCartItemToCart.Quantity + 1;
                //ViewBag.cartCount += 1;

                
                if (matchCartItemToCart.Quantity > (productTemp.Quantity))
                {
                    matchCartItemToCart.Quantity = productTemp.Quantity;
                }
                else
                {
                    totalQuantity++;
                }

                matchCartItemToCart.Price = productTemp.Quantity * productTemp.Price;

                //db.SaveChanges();

                db.Entry(matchCartItemToCart).State = EntityState.Modified;
                db.SaveChanges();


                //return RedirectToAction("Index", "ProductTable");
                return Json( new List<int> { (matchCartItemToCart.Quantity == null) ? -4000 : (int)matchCartItemToCart.Quantity, totalQuantity });
            }
            else
            {
                var scpt = new ShoppingCartProductTable
                {
                    ShoppingCartID = matchCartToUserQuery.ShoppingCartID,
                    ProductID = id,
                    Quantity = 1,
                    Price = (getPriceQuery.Single().Equals(null)) ? (decimal)(1.01) : getPriceQuery.Single()
                };

                if (scpt.Quantity > (productTemp.Quantity))
                {
                    scpt.Quantity = 0;
                    return Json(new List<int> { 0, totalQuantity });
                }
                else
                {
                    totalQuantity++;
                    db.ShoppingCartProductTable.Add(scpt);
                    db.SaveChanges();
                    return Json(new List<int> { 1, totalQuantity });
                }

                
                //ViewBag.cartCount += 1;
               
                //return RedirectToAction("Index", "ProductTable");
                
            }
        }

        public ActionResult Checkout()
        {
            var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
            var matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();
            var matchCartItemToCart = (from cartItemLinq in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == matchCartToUserQuery.ShoppingCartID) select cartItemLinq);

            OrderTable ot = new OrderTable
            {
                UserID = userQuery.UserID,
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
            return RedirectToAction("Address", "OrderAdminDetails");
        }


        //[WebMethod]
        //public /*static*/ ActionResult UpdateQuantityNooooo(int id, int quantity)
        //{
        //    try
        //    {
        //        ShoppingCartProductTable cartItem = getCartItem(id);
        //        cartItem.Quantity = quantity;
        //        cartItem.Price = cartItem.Price * cartItem.Quantity;
        //        db.SaveChanges();
        //        return Json(cartItem);
        //    }
        //    catch
        //    {
        //        return Content("failure");
        //    }
        //}

        /*
        [HttpPost]
        public ActionResult UpdateQuantity()
        {
            return View();

        }   */

        /*
    [HttpPost]
    public int? UpdateQuantity(int id, int quantity)
    {
        try
        {
            Console.WriteLine("Hey there, and welcome to the C# method.");
            ShoppingCartProductTable cartItem = getCartItem(id);
            int? originalItemQuantity = cartItem.Quantity;
            cartItem.Quantity = quantity;
            var itemType = (from qualityQualifier in db.ProductTable.Where(QQ => QQ.ProductID == id) select qualityQualifier).FirstOrDefault();
            if (cartItem.Quantity > (itemType.Quantity))
            {
                cartItem.Quantity = itemType.Quantity;
            }
            int? quantityChange = originalItemQuantity - cartItem.Quantity;
            if(cartItem.Quantity < 1)
            {
                db.ShoppingCartProductTable.Remove(cartItem);
            }
            else
            {
                cartItem.Price = itemType.Price * cartItem.Quantity;
                db.Entry(cartItem).State = EntityState.Modified;
            }
            db.SaveChanges();
            return quantityChange;
        }
        catch
        {
            Console.WriteLine("Null reference exception, yo.");
            return null;
        }
    }
    */

        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            //try
            //{
                int? totalQuantity = 0;
                //var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
                //var matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();
                //var matchCartItemToCart = (from cartItemLinq in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == matchCartToUserQuery.ShoppingCartID) select cartItemLinq).ToList();
                //for (int i = 0; i < matchCartItemToCart.Count(); i++)
                //{
                //    totalQuantity += matchCartItemToCart[i].Quantity;
                //}
                //if (totalQuantity == null)
                //{
                //    return -99999;
                //}
                //else
                //{
                //    return (int)totalQuantity;
                //}

                
                if(quantity < 0)
                {
                    quantity = 0;
                }
                ShoppingCartProductTable cartItem = getCartItem(id);
                cartItem.Quantity = quantity;
                var itemType = (from qualityQualifier in db.ProductTable.Where(QQ => QQ.ProductID == id) select qualityQualifier).FirstOrDefault();
                if (cartItem.Quantity > (itemType.Quantity))
                {
                    cartItem.Quantity = itemType.Quantity;
                }

                var allCartItems = (from carts in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == cartItem.ShoppingCartID) select carts).ToList();
                for(int i = 0; i < allCartItems.Count(); i++)
                {
                    totalQuantity += (int)allCartItems[i].Quantity;
                }

                    

                if (quantity < 1)
                {
                    db.ShoppingCartProductTable.Remove(cartItem);
                    db.SaveChanges();
                    return Json(new List<int> { 0, (int)((totalQuantity != null)? totalQuantity: 0) });
                }
                else
                {
                    cartItem.Price = itemType.Price * cartItem.Quantity;
                    db.Entry(cartItem).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new List<int> { quantity, (int)totalQuantity });
                }
            //}
            //catch
            //{
            //    Console.WriteLine("Null reference exception, yo.");
            //    return -99999;
            //}
        }

        //this is so that the number of cart items updates...
        [HttpGet]
        public void refreshLoginPartial()
        {
            //return Json(PartialView("~/Views/Shared/_LoginPartial.cshtml"));

            //return Html.Raw(JsonConvert.SerializeObject(Html.Partial("_LoginPartial")))
            //return RenderViewToString(PartialView("Shared/_LoginPartial"));
        }


        public ActionResult PlaceOrder(int id)
        {
            return RedirectToAction("ChooseAddress", "PlaceOrder");
        }

        // GET: ShoppingCartProductTable/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShoppingCartProductTable shoppingCartProductTable = db.ShoppingCartProductTable.Find(id);
            if (shoppingCartProductTable == null)
            {
                return HttpNotFound();
            }
            return View(shoppingCartProductTable);
        }

        //// GET: ShoppingCartProductTable/Create
        //public ActionResult Create()
        //{
        //    ViewBag.ProductID = new SelectList(db.ProductTable, "ProductID", "ProductName");
        //    ViewBag.ShoppingCartID = new SelectList(db.ShoppingCartTable, "ShoppingCartID", "CreatedBy");
        //    return View();
        //}

        //// POST: ShoppingCartProductTable/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "ShoppingCartProductID,ShoppingCartID,ProductID,Quantity,Price,CreatedBy,DateModified,ModifiedBy,ImageFile")] ShoppingCartProductTable shoppingCartProductTable)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.ShoppingCartProductTable.Add(shoppingCartProductTable);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.ProductID = new SelectList(db.ProductTable, "ProductID", "ProductName", shoppingCartProductTable.ProductID);
        //    ViewBag.ShoppingCartID = new SelectList(db.ShoppingCartTable, "ShoppingCartID", "CreatedBy", shoppingCartProductTable.ShoppingCartID);
        //    return View(shoppingCartProductTable);
        //}

        // GET: ShoppingCartProductTable/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ShoppingCartProductTable shoppingCartProductTable = db.ShoppingCartProductTable.Find(id);
        //    if (shoppingCartProductTable == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.ProductID = new SelectList(db.ProductTable, "ProductID", "ProductName", shoppingCartProductTable.ProductID);
        //    ViewBag.ShoppingCartID = new SelectList(db.ShoppingCartTable, "ShoppingCartID", "CreatedBy", shoppingCartProductTable.ShoppingCartID);
        //    return View(shoppingCartProductTable);
        //}

        //// POST: ShoppingCartProductTable/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "ShoppingCartProductID,ShoppingCartID,ProductID,Quantity,Price,CreatedBy,DateModified,ModifiedBy,ImageFile")] ShoppingCartProductTable shoppingCartProductTable)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(shoppingCartProductTable).State = EntityState.Modified;


        //        var productTemp = (from qualityQualifier in db.ProductTable.Where(QQ => QQ.ProductID == shoppingCartProductTable.ProductID) select qualityQualifier).FirstOrDefault();

        //        if (shoppingCartProductTable.Quantity > (productTemp.Quantity))
        //        {
        //            shoppingCartProductTable.Quantity = productTemp.Quantity;
        //        }

        //        shoppingCartProductTable.Price = shoppingCartProductTable.Quantity * productTemp.Price;

        //        //ViewBag.cartCount += (shoppingCartProductTable.Quantity - ViewBag.oldCartItemQuantity);


        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.ProductID = new SelectList(db.ProductTable, "ProductID", "ProductName", shoppingCartProductTable.ProductID);
        //    ViewBag.ShoppingCartID = new SelectList(db.ShoppingCartTable, "ShoppingCartID", "CreatedBy", shoppingCartProductTable.ShoppingCartID);
        //    return View(shoppingCartProductTable);
        //}

        // GET: ShoppingCartProductTable/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShoppingCartProductTable shoppingCartProductTable = db.ShoppingCartProductTable.Find(id);
            if (shoppingCartProductTable == null)
            {
                return HttpNotFound();
            }
            return View(shoppingCartProductTable);
        }

        // POST: ShoppingCartProductTable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ShoppingCartProductTable shoppingCartProductTable = db.ShoppingCartProductTable.Find(id);
            //ViewBag.cartCount -= shoppingCartProductTable.Quantity;
            db.ShoppingCartProductTable.Remove(shoppingCartProductTable);
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

        public double getPrice()
        {
            try
            {
                var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
                var matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();
                double totalPrice = 0;
                var theProduct = (from item in db.ShoppingCartProductTable.Where(t => t.ShoppingCartID == matchCartToUserQuery.ShoppingCartID) select item);
                foreach (var item in theProduct)
                {
                    totalPrice += (double)(from price in db.ProductTable.Where(u => u.ProductID == item.ProductID) select price.Price).FirstOrDefault() * (double)item.Quantity;
                }
                return totalPrice;
            }
            catch
            {
                return 0;
            }
        }

        
        public List<ShoppingCartProductTable> getCartItems()
        {
            try
            {
                var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
                var matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();
                var matchCartItemToCart = (from cartItemLinq in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == matchCartToUserQuery.ShoppingCartID) select cartItemLinq).ToList();
                return matchCartItemToCart;
            }
            catch
            {
                return null;
            }
        }

        public ShoppingCartTable getCart()
        {
            var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
            var matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();
            return matchCartToUserQuery;
        }

        public ShoppingCartProductTable getCartItem(int id)
        {
            var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
            var matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();
            var matchCartItemToCart = (from cartItemLinq in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == matchCartToUserQuery.ShoppingCartID && c.ProductID == id) select cartItemLinq).FirstOrDefault();
            return matchCartItemToCart;
        }

        /*
        public int getQuantity(int id)
        {
            int? totalQuantity = 0;
            var userQuery = (from userLinq in db.UserTable.Where(userLinq => String.Equals(userLinq.UserName, System.Web.HttpContext.Current.User.Identity.Name)) select userLinq).FirstOrDefault();
            var matchCartToUserQuery = (from shoppingCartLinq in db.ShoppingCartTable.Where(s => s.UserID == userQuery.UserID) select shoppingCartLinq).FirstOrDefault();
            var matchCartItemToCart = (from cartItemLinq in db.ShoppingCartProductTable.Where(c => c.ShoppingCartID == matchCartToUserQuery.ShoppingCartID) select cartItemLinq).ToList();
            for(int i = 0; i < matchCartItemToCart.Count(); i++)
            {
                totalQuantity += matchCartItemToCart[i].Quantity;
            }
            if(totalQuantity == null)
            {
                return -99999;
            }
            else
            {
                return (int)totalQuantity;
            }
        }*/
    }
}
