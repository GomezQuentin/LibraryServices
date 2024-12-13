using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class LibraryTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string BookId { get; set; }
        public string TransactionType { get; set; } // e.g., "CheckOut", "Return"
        public DateTime Date { get; set; }
    }
}