using MvcSf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSf.ViewModels
{
    public class MasterViewModel
    {
        public AddressTable AddressTable { get; set; }
        public OrderProductTable OrderProductTable { get; set; }
        public OrderTable OrderTable { get; set; }
        public ProductTable ProductTable { get; set; }
        public ShoppingCartProductTable ShoppingCartProductTable { get; set; }
        public ShoppingCartTable ShoppingCartTable { get; set; }
        public StateTable StateTable { get; set; }
        public StatusTable StatusTable { get; set; }
        public sysdiagrams sysdiagrams { get; set; }
        public UserTable UserTable { get; set; }
    }
}