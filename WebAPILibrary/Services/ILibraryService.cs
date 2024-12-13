using DAL.Models;

namespace WebAPILibrary.Services
{
    public interface ILibraryService
    {
        bool CheckOutBook(int userId, string bookId);
        bool ReturnBook(int userId, string bookId);
        decimal GetOutstandingFees(int userId);
        List<LibraryTransaction> GetUserLibraryTransactions(int userId);
        //Extra funtctions 
        string ProcessFeePayment(int userId, decimal paymentAmount);
        string RenewBook(int userId, string bookId);
        Dictionary<string, string> CheckOutBooks(int userId, List<string> bookIds);

    }
}
