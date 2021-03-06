//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MvcSf.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductTable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProductTable()
        {
            this.OrderProductTable = new HashSet<OrderProductTable>();
            this.ShoppingCartProductTable = new HashSet<ShoppingCartProductTable>();
        }
    
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public Nullable<bool> IsPublished { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string ImageFile { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public string DateModified { get; set; }
        public string ModifiedBy { get; set; }
        public byte[] ProductImage { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderProductTable> OrderProductTable { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShoppingCartProductTable> ShoppingCartProductTable { get; set; }
    }
}
