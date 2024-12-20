using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPILibrary.Controllers;
using WebAPILibrary.Services;
using Xunit;

public class LibraryControllerTests
{
    private readonly Mock<ILibraryService> _mockLibraryService;
    private readonly LibraryController _controller;

    public LibraryControllerTests()
    {
        _mockLibraryService = new Mock<ILibraryService>();
        _controller = new LibraryController(_mockLibraryService.Object);
    }

    [Fact]
    public void GetOutstandingFees_UserExists_ReturnsOk()
    {
        // Arrange
        int userId = 1;
        decimal expectedFees = 25m;
        _mockLibraryService.Setup(service => service.GetOutstandingFees(userId)).Returns(expectedFees);

        // Act
        var result = _controller.GetOutstandingFees(userId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }
    
    [Fact]
    public void GetOutstandingFees_UserExists_ReturnsCorrectFees()
    {
        // Arrange
        int userId = 1;
        decimal expectedFees = 25m;
        _mockLibraryService.Setup(service => service.GetOutstandingFees(userId)).Returns(expectedFees);

        // Act
        var result = _controller.GetOutstandingFees(userId) as ObjectResult;

        // Assert
        Assert.NotNull(result);

        var value = result.Value;
        Assert.NotNull(value);

        var userIdProperty = value.GetType().GetProperty("UserId");
        var outstandingFeesProperty = value.GetType().GetProperty("OutstandingFees");

        Assert.NotNull(userIdProperty);
        Assert.NotNull(outstandingFeesProperty);

        Assert.Equal(userId, (int)userIdProperty.GetValue(value));
        Assert.Equal(expectedFees, (decimal)outstandingFeesProperty.GetValue(value));
    }

    [Fact]
    public void GetOutstandingFees_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        int userId = 999;
        _mockLibraryService.Setup(service => service.GetOutstandingFees(userId)).Throws(new ArgumentException("User not found."));

        // Act
        var result = _controller.GetOutstandingFees(userId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result?.StatusCode);
    }

    [Fact]
    public void GetOutstandingFees_ServiceThrowsException_500ServerError()
    {
        // Arrange
        int userId = 1;
        _mockLibraryService.Setup(service => service.GetOutstandingFees(userId)).Throws(new Exception("Service error"));

        // Act
        var result = _controller.GetOutstandingFees(userId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result?.StatusCode);
    }

    [Fact]
    public void CheckOutBook_ValidInputs_ReturnsOk()
    {
        // Arrange
        var userId = 1;
        var bookId = "123";
        _mockLibraryService.Setup(service => service.CheckOutBook(userId, bookId)).Returns(true);

        // Act
        var result = _controller.CheckOutBook(userId, bookId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void CheckOutBook_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        int userId = 999;
        string bookId = "123";
        _mockLibraryService
            .Setup(service => service.CheckOutBook(userId, bookId))
            .Throws(new ArgumentException("Invalid userId or bookId"));

        // Act
        var result = _controller.CheckOutBook(userId, bookId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }


    [Fact]
    public void CheckOutBook_BookNotFound_ReturnsNotFound()
    {
        // Arrange
        int userId = 1;
        string bookId = "999";
        _mockLibraryService
            .Setup(service => service.CheckOutBook(userId, bookId))
            .Throws(new ArgumentException("Invalid userId or bookId"));

        // Act
        var result = _controller.CheckOutBook(userId, bookId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }


    [Fact]
    public void CheckOutBook_BookNotAvailable_ReturnsBadRequest()
    {
        // Arrange
        int userId = 1;
        string bookId = "123";
        _mockLibraryService
            .Setup(service => service.CheckOutBook(userId, bookId))
            .Throws(new ArgumentException("Book is not available"));

        // Act
        var result = _controller.CheckOutBook(userId, bookId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void CheckOutBook_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        int userId = 1;
        string bookId = "123";
        _mockLibraryService
            .Setup(service => service.CheckOutBook(userId, bookId))
            .Throws(new Exception("Unexpected error."));

        // Act
        var result = _controller.CheckOutBook(userId, bookId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public void ReturnBook_ValidInputs_ReturnsOk()
    {
        // Arrange
        int userId = 1;
        string bookId = "123";
        _mockLibraryService.Setup(service => service.ReturnBook(userId, bookId)).Returns(true);

        // Act
        var result = _controller.ReturnBook(userId, bookId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void ReturnBook_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        int userId = 999;
        string bookId = "123";
        _mockLibraryService.Setup(service => service.ReturnBook(userId, bookId))
            .Throws(new ArgumentException("User not found."));

        // Act
        var result = _controller.ReturnBook(userId, bookId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void ReturnBook_BookNotFound_ReturnsNotFound()
    {
        // Arrange
        int userId = 1;
        string bookId = "999";
        _mockLibraryService.Setup(service => service.ReturnBook(userId, bookId))
            .Throws(new ArgumentException("Book not found."));

        // Act
        var result = _controller.ReturnBook(userId, bookId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void ReturnBook_NoValidCheckoutRecord_ReturnsBadRequest()
    {
        // Arrange
        int userId = 1;
        string bookId = "123";
        _mockLibraryService.Setup(service => service.ReturnBook(userId, bookId))
            .Throws(new InvalidOperationException("No valid checkout record found for this book and user."));

        // Act
        var result = _controller.ReturnBook(userId, bookId) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void ReturnBook_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        int userId = 1;
        string bookId = "123";

        _mockLibraryService
            .Setup(service => service.ReturnBook(userId, bookId))
            .Throws(new Exception("Unexpected error"));

        // Act
        var result = _controller.ReturnBook(userId, bookId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public void GetUserLibraryTransactions_UserHasTransactions_ReturnsOk()
    {
        // Arrange
        int userId = 1;
        var transactions = new List<LibraryTransaction>
    {
        new LibraryTransaction { Id = 1, BookId = "123", TransactionType = "Checkout", Date = DateTime.Now.AddDays(-5) },
        new LibraryTransaction { Id = 2, BookId = "456", TransactionType = "Return", Date = DateTime.Now.AddDays(-2) }
    };

        _mockLibraryService
            .Setup(service => service.GetUserLibraryTransactions(userId))
            .Returns(transactions);

        // Act
        var result = _controller.GetUserLibraryTransactions(userId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void GetUserLibraryTransactions_UserHasTransactions_ReturnsRightAmountOfTransaction()
    {
        // Arrange
        int userId = 1;
        var transactions = new List<LibraryTransaction>
    {
        new LibraryTransaction { Id = 1, BookId = "123", TransactionType = "Checkout", Date = DateTime.Now.AddDays(-5) },
        new LibraryTransaction { Id = 2, BookId = "456", TransactionType = "Return", Date = DateTime.Now.AddDays(-2) }
    };

        _mockLibraryService
            .Setup(service => service.GetUserLibraryTransactions(userId))
            .Returns(transactions);

        // Act
        var result = _controller.GetUserLibraryTransactions(userId) as ObjectResult;

        // Assert
        var value = result.Value;
        Assert.NotNull(value);

        var userIdProperty = value.GetType().GetProperty("UserId");
        var transactionsProperty = value.GetType().GetProperty("Transactions");

        Assert.NotNull(userIdProperty);
        Assert.NotNull(transactionsProperty);

        Assert.Equal(userId, (int)userIdProperty.GetValue(value));

        var transactionsValue = transactionsProperty.GetValue(value) as IEnumerable<object>;
        Assert.NotNull(transactionsValue);
        Assert.Equal(2, transactionsValue.Count());
    }

    [Fact]
    public void GetUserLibraryTransactions_UserHasNoTransactions_ReturnsOk()
    {
        // Arrange
        int userId = 1;
        var transactions = new List<LibraryTransaction>();

        _mockLibraryService
            .Setup(service => service.GetUserLibraryTransactions(userId))
            .Returns(transactions);

        // Act
        var result = _controller.GetUserLibraryTransactions(userId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void GetUserLibraryTransactions_UserHasNoTransactions_ReturnsEmptyList()
    {
        // Arrange
        int userId = 1;
        var transactions = new List<LibraryTransaction>();

        _mockLibraryService
            .Setup(service => service.GetUserLibraryTransactions(userId))
            .Returns(transactions);

        // Act
        var result = _controller.GetUserLibraryTransactions(userId) as OkObjectResult;

        // Assert
        var value = result.Value;
        Assert.NotNull(value);

        var userIdProperty = value.GetType().GetProperty("UserId");
        var transactionsProperty = value.GetType().GetProperty("Transactions");

        Assert.NotNull(userIdProperty);
        Assert.NotNull(transactionsProperty);

        Assert.Equal(userId, (int)userIdProperty.GetValue(value));

        var transactionsValue = transactionsProperty.GetValue(value) as IEnumerable<object>;
        Assert.NotNull(transactionsValue);
        Assert.Empty(transactionsValue);
    }
    [Fact]
    public void GetUserLibraryTransactions_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        int userId = 999;

        _mockLibraryService
            .Setup(service => service.GetUserLibraryTransactions(userId))
            .Throws(new ArgumentException("User not found."));

        // Act
        var result = _controller.GetUserLibraryTransactions(userId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void GetUserLibraryTransactions_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        int userId = 1;

        _mockLibraryService
            .Setup(service => service.GetUserLibraryTransactions(userId))
            .Throws(new Exception("Unexpected error."));

        // Act
        var result = _controller.GetUserLibraryTransactions(userId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public void ProcessFeePayment_ValidPayment_ReturnsOk()
    {
        // Arrange
        int userId = 1;
        decimal paymentAmount = 25.00m;
        string successMessage = "Payment processed successfully. Outstanding fees have been cleared.";

        _mockLibraryService
            .Setup(service => service.ProcessFeePayment(userId, paymentAmount))
            .Returns(successMessage);

        // Act
        var result = _controller.ProcessFeePayment(userId, paymentAmount) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void ProcessFeePayment_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        int userId = 999;
        decimal paymentAmount = 25.00m;

        _mockLibraryService
            .Setup(service => service.ProcessFeePayment(userId, paymentAmount))
            .Throws(new ArgumentException("User not found."));

        // Act
        var result = _controller.ProcessFeePayment(userId, paymentAmount) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void ProcessFeePayment_InvalidPaymentAmount_ReturnsBadRequest()
    {
        // Arrange
        int userId = 1;
        decimal paymentAmount = 20.00m;
        string failureMessage = "Payment failed: Payment amount does not match the outstanding fees. Outstanding Fees: 25";

        _mockLibraryService
            .Setup(service => service.ProcessFeePayment(userId, paymentAmount))
            .Returns(failureMessage);

        // Act
        var result = _controller.ProcessFeePayment(userId, paymentAmount) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void ProcessFeePayment_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        int userId = 1;
        decimal paymentAmount = 25.00m;

        _mockLibraryService
            .Setup(service => service.ProcessFeePayment(userId, paymentAmount))
            .Throws(new Exception("Unexpected error."));

        // Act
        var result = _controller.ProcessFeePayment(userId, paymentAmount) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public void RenewBook_ValidRenewal_ReturnsOk()
    {
        // Arrange
        int userId = 1;
        string bookId = "123";
        string successMessage = "Book renewed successfully. New due date: 12/20/2023";

        _mockLibraryService
            .Setup(service => service.RenewBook(userId, bookId))
            .Returns(successMessage);

        // Act
        var result = _controller.RenewBook(userId, bookId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void RenewBook_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        int userId = 999;
        string bookId = "123";

        _mockLibraryService
            .Setup(service => service.RenewBook(userId, bookId))
            .Throws(new ArgumentException("User not found."));

        // Act
        var result = _controller.RenewBook(userId, bookId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void RenewBook_BookDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        int userId = 1;
        string bookId = "999";

        _mockLibraryService
            .Setup(service => service.RenewBook(userId, bookId))
            .Throws(new ArgumentException("Book not found."));

        // Act
        var result = _controller.RenewBook(userId, bookId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void RenewBook_NoCheckoutRecord_ReturnsBadRequest()
    {
        // Arrange
        int userId = 1;
        string bookId = "123";

        _mockLibraryService
            .Setup(service => service.RenewBook(userId, bookId))
            .Throws(new InvalidOperationException("No valid checkout record found for this book and user."));

        // Act
        var result = _controller.RenewBook(userId, bookId) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void RenewBook_AlreadyRenewed_ReturnsBadRequest()
    {
        // Arrange
        int userId = 1;
        string bookId = "123";
        string failureMessage = "Renewal failed: Book has already been renewed.";

        _mockLibraryService
            .Setup(service => service.RenewBook(userId, bookId))
            .Returns(failureMessage);

        // Act
        var result = _controller.RenewBook(userId, bookId) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void RenewBook_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        int userId = 1;
        string bookId = "123";

        _mockLibraryService
            .Setup(service => service.RenewBook(userId, bookId))
            .Throws(new Exception("Unexpected error occurred"));

        // Act
        var result = _controller.RenewBook(userId, bookId) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
    }


    [Fact]
    public void CheckOutBooks_AllBooksAvailable_ReturnsOk()
    {
        // Arrange
        int userId = 1;
        var bookIds = new List<string> { "123", "456" };
        var mockResults = new Dictionary<string, string>
    {
        { "123", "Checkout successful." },
        { "456", "Checkout successful." }
    };

        _mockLibraryService
            .Setup(service => service.CheckOutBooks(userId, bookIds))
            .Returns(mockResults);

        // Act
        var result = _controller.CheckOutBooks(userId, bookIds) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void CheckOutBooks_SomeBooksUnavailable_ReturnsOk()
    {
        // Arrange
        int userId = 1;
        var bookIds = new List<string> { "123", "456" };
        var mockResults = new Dictionary<string, string>
    {
        { "123", "Checkout successful." },
        { "456", "Book is not available." }
    };

        _mockLibraryService
            .Setup(service => service.CheckOutBooks(userId, bookIds))
            .Returns(mockResults);

        // Act
        var result = _controller.CheckOutBooks(userId, bookIds) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void CheckOutBooks_BookDoesNotExist_ReturnsOk()
    {
        // Arrange
        int userId = 1;
        var bookIds = new List<string> { "123", "999" };
        var mockResults = new Dictionary<string, string>
    {
        { "123", "Checkout successful." },
        { "999", "Book not found." }
    };

        _mockLibraryService
            .Setup(service => service.CheckOutBooks(userId, bookIds))
            .Returns(mockResults);

        // Act
        var result = _controller.CheckOutBooks(userId, bookIds) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void CheckOutBooks_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        int userId = 999;
        var bookIds = new List<string> { "123", "456" };

        _mockLibraryService
            .Setup(service => service.CheckOutBooks(userId, bookIds))
            .Throws(new ArgumentException("User not found."));

        // Act
        var result = _controller.CheckOutBooks(userId, bookIds) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void CheckOutBooks_EmptyBookList_ReturnsOk()
    {
        // Arrange
        int userId = 1;
        var bookIds = new List<string>(); // Empty list
        var mockResults = new Dictionary<string, string>(); // Empty results

        _mockLibraryService
            .Setup(service => service.CheckOutBooks(userId, bookIds))
            .Returns(mockResults);

        // Act
        var result = _controller.CheckOutBooks(userId, bookIds) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void CheckOutBooks_EmptyBookList_ReturnsEmptyResults()
    {
        // Arrange
        int userId = 1;
        var bookIds = new List<string>();
        var mockResults = new Dictionary<string, string>();

        _mockLibraryService
            .Setup(service => service.CheckOutBooks(userId, bookIds))
            .Returns(mockResults);

        // Act
        var result = _controller.CheckOutBooks(userId, bookIds) as OkObjectResult;

        // Assert
        Assert.NotNull(result);

        var value = result.Value;
        Assert.NotNull(value);

        var userIdProperty = value.GetType().GetProperty("UserId");
        var resultsProperty = value.GetType().GetProperty("Results");

        Assert.NotNull(userIdProperty);
        Assert.NotNull(resultsProperty);

        var resultsValue = resultsProperty.GetValue(value) as IDictionary<string, string>;
        Assert.NotNull(resultsValue);
        Assert.Empty(resultsValue);
    }

    [Fact]
    public void CheckOutBooks_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        int userId = 1;
        var bookIds = new List<string> { "123", "456" };

        _mockLibraryService
            .Setup(service => service.CheckOutBooks(userId, bookIds))
            .Throws(new Exception("Unexpected error occurred"));

        // Act
        var result = _controller.CheckOutBooks(userId, bookIds) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
    }
}