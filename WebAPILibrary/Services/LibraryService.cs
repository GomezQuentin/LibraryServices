using DAL.Models;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace WebAPILibrary.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly ProjectContext _dbContext;

        public LibraryService(ProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public decimal GetOutstandingFees(int userId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }
            return user.Fees;
        }

        public bool CheckOutBook(int userId, string bookId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            var book = _dbContext.Books.FirstOrDefault(b => b.Id == bookId);

            if (user == null || book == null)
            {
                throw new ArgumentException("Invalid userId or bookId");
            }

            if (!book.Available)
            {
                throw new ArgumentException("Book not available");
            }

            book.Available = false;

            var transaction = new LibraryTransaction
            {
                UserId = userId,
                BookId = bookId,
                TransactionType = "Checkout",
                Date = DateTime.Now
            };

            _dbContext.LibraryTransactions.Add(transaction);

            _dbContext.SaveChanges();

            return true;
        }

        public bool ReturnBook(int userId, string bookId)
        {
            var book = _dbContext.Books.FirstOrDefault(b => b.Id == bookId);
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null || book == null)
            {
                throw new ArgumentException("Invalid userId or bookId");
            }

            if (book.Available)
            {
                throw new InvalidOperationException("The book cannot be returned because it was not checked out by the current user.");
            }

            var checkoutTransaction = _dbContext.LibraryTransactions
                .Where(t => t.UserId == userId && t.BookId == bookId && t.TransactionType == "Checkout")
                .OrderByDescending(t => t.Date)
                .FirstOrDefault();

            if (checkoutTransaction == null)
            {
                throw new InvalidOperationException("No valid checkout record found for this book and user.");
            }

            var totalBorrowingDays = book.BorrowingDays;
            var renewalTransactions = _dbContext.LibraryTransactions
                .Where(t => t.UserId == userId && t.BookId == bookId && t.TransactionType == "Renew")
                .OrderBy(t => t.Date)
                .ToList();

            totalBorrowingDays += renewalTransactions.Count * book.BorrowingDays;

            var dueDate = checkoutTransaction.Date.AddDays(totalBorrowingDays);

            var returnDate = DateTime.Now;
            if (returnDate > dueDate)
            {
                var overdueDays = (returnDate - dueDate).Days;
                var lateFee = overdueDays * book.FeePrice;
                user.Fees += lateFee;
            }

            book.Available = true;

            _dbContext.LibraryTransactions.Add(new LibraryTransaction
            {
                UserId = userId,
                BookId = bookId,
                TransactionType = "Return",
                Date = returnDate
            });

            _dbContext.SaveChanges();
            return true;
        }


        public List<LibraryTransaction> GetUserLibraryTransactions(int userId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }
            return _dbContext.LibraryTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToList();
        }

        public string ProcessFeePayment(int userId, decimal paymentAmount)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            if (paymentAmount != user.Fees)
            {
                return $"Payment failed: Payment amount does not match the outstanding fees. Outstanding Fees: {user.Fees}";
            }

            user.Fees = 0;

            _dbContext.SaveChanges();

            return "Payment processed successfully. Outstanding fees have been cleared.";
        }
        public string RenewBook(int userId, string bookId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            var book = _dbContext.Books.FirstOrDefault(b => b.Id == bookId);

            if (user == null || book == null)
            {
                throw new ArgumentException("Invalid userId or bookId");
            }

            var transaction = _dbContext.LibraryTransactions
                .Where(t => t.UserId == userId && t.BookId == bookId && t.TransactionType == "Checkout")
                .OrderByDescending(t => t.Date)
                .FirstOrDefault();

            if (transaction == null)
            {
                throw new InvalidOperationException("No valid checkout record found for this book and user.");
            }

            var renewalTransaction = _dbContext.LibraryTransactions
                .Where(t => t.UserId == userId && t.BookId == bookId && t.TransactionType == "Renew")
                .OrderByDescending(t => t.Date)
                .FirstOrDefault();

            if (renewalTransaction != null)
            {
                return "Renewal failed: Book has already been renewed.";
            }

            var newDueDate = transaction.Date.AddDays(book.BorrowingDays).AddDays(book.BorrowingDays);

            _dbContext.LibraryTransactions.Add(new LibraryTransaction
            {
                UserId = userId,
                BookId = bookId,
                TransactionType = "Renew",
                Date = DateTime.Now
            });

            _dbContext.SaveChanges();

            return $"Book renewed successfully. New due date: {newDueDate.ToShortDateString()}";
        }

        public Dictionary<string, string> CheckOutBooks(int userId, List<string> bookIds)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var results = new Dictionary<string, string>();

            foreach (var bookId in bookIds)
            {
                try
                {
                    var success = CheckOutBook(userId, bookId);

                    results[bookId] = success ? "Checkout successful." : "Checkout failed.";
                }
                catch (ArgumentException ex)
                {
                    results[bookId] = ex.Message;
                }
            }

            return results;
        }

    }
}