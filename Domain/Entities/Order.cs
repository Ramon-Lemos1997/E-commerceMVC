using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Domain.Entities
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public string UserId { get; set; }   
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public bool PaymentConfirmed { get; set; } = false;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } 

        
    }
}
