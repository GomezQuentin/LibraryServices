using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Book
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public decimal FeePrice { get; set; }
        public int BorrowingDays { get; set; } // Days allowed for borrowing
        public bool Available { get; set; }
    }
}
