using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq;
using WebAPILibrary.Services;
using Xunit;

public class LibraryServiceTests
{
    [Fact]
    public void GetOutstandingFees_UserExistsWithFees_ReturnsCorrectFees()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserExistsWithFees")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User
            {
                Id = 1,
                Name = "Test User",
                FirstName = "John",
                Fees = 25m
            });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.GetOutstandingFees(1);

            // Assert
            Assert.Equal(25m, result);
        }
    }

    [Fact]
    public void GetOutstandingFees_UserExistsWithoutFees_ReturnsZero()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserExistsWithoutFees")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User
            {
                Id = 2,
                Name = "No Fees User",
                FirstName = "Jane",
                Fees = 0m
            });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.GetOutstandingFees(2);

            // Assert
            Assert.Equal(0m, result);
        }
    }

    [Fact]
    public void GetOutstandingFees_UserDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserDoesNotExist")
            .Options;

        using (var context = new ProjectContext(options))
        {
            // No users added
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => libraryService.GetOutstandingFees(999)); // Non-existing user Id
        }
    }

    [Fact]
    public void GetOutstandingFees_MultipleUsers_ReturnsCorrectFeesForSpecifiedUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_MultipleUsers")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.AddRange(
                new User { Id = 3, Name = "User A", FirstName = "Alice", Fees = 15m },
                new User { Id = 4, Name = "User B", FirstName = "Bob", Fees = 20m }
            );
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.GetOutstandingFees(4);

            // Assert
            Assert.Equal(20m, result);
        }
    }

    [Fact]
    public void GetOutstandingFees_UserHasNegativeFees_ReturnsNegativeValue()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserNegativeFees")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User
            {
                Id = 5,
                Name = "Negative Fees User",
                FirstName = "Chris",
                Fees = -10m
            });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.GetOutstandingFees(5);

            // Assert
            Assert.Equal(-10m, result);
        }
    }

    [Fact]
    public void CheckOutBook_BookDoesNotExist_ReturnsError()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_BookDoesNotExist")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 8, Name = "John", FirstName = "Doe", Fees = 0 });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var exception = Assert.Throws<ArgumentException>(() => libraryService.CheckOutBook(8, "999"));

            // Assert
            Assert.Equal("Invalid userId or bookId", exception.Message);
        }
    }

    [Fact]
    public void CheckOutBook_UserDoesNotExist_ReturnsError()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserDoesNotExist")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Books.Add(new Book { Id = "1234", Title = "Test Book", Available = true });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var exception = Assert.Throws<ArgumentException>(() => libraryService.CheckOutBook(999, "123"));

            // Assert
            Assert.Equal("Invalid userId or bookId", exception.Message);
        }
    }

    [Fact]
    public void CheckOutBook_BookNotAvailable_ReturnsError()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_BookNotAvailable")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName="Doe", Fees = 0 });
            context.Books.Add(new Book { Id = "123", Title = "Test Book", Available = false });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var exception = Assert.Throws<ArgumentException>(() => libraryService.CheckOutBook(1, "123"));

            // Assert
            Assert.Equal("Book not available", exception.Message);
        }
    }

    [Fact]
    public void CheckOutBook_ValidInputs_UpdatesAvailability()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UpdatesAvailability")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName="Doe", Fees = 0 });
            context.Books.Add(new Book { Id = "123", Title = "Test Book", Available = true });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.CheckOutBook(1, "123");

            // Assert
            Assert.True(result);
            var book = context.Books.FirstOrDefault(b => b.Id == "123");
            Assert.NotNull(book);
            Assert.False(book.Available);
        }
    }

    [Fact]
    public void CheckOutBook_MultipleBooks_UpdatesAvailability()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_MultipleBooksCheckout")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 5, Name = "John", FirstName = "Doe", Fees = 0 });
            context.Books.AddRange(
                new Book { Id = "e", Title = "Book 1", Available = true },
                new Book { Id = "f", Title = "Book 2", Available = true }
            );
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result1 = libraryService.CheckOutBook(5, "e");
            var result2 = libraryService.CheckOutBook(5, "f");

            // Assert
            Assert.True(result1);
            Assert.True(result2);

            var book1 = context.Books.FirstOrDefault(b => b.Id == "e");
            var book2 = context.Books.FirstOrDefault(b => b.Id == "f");

            Assert.NotNull(book1);
            Assert.NotNull(book2);

            Assert.False(book1.Available);
            Assert.False(book2.Available);
        }
    }


    [Fact]
    public void ReturnBook_UserDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserDoesNotExist")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Books.Add(new Book
            {
                Id = "123",
                Title = "Test Book",
                BorrowingDays = 14,
                FeePrice = 2.5m,
                Available = false
            });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => libraryService.ReturnBook(999, "123"));
            Assert.Equal("Invalid userId or bookId", exception.Message);
        }
    }

    [Fact]
    public void ReturnBook_BookDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_BookDoesNotExist")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User
            {
                Id = 2,
                Name = "John Doe",
                FirstName = "John",
                Fees = 0m
            });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => libraryService.ReturnBook(1, "999")); // Non-existent bookId
            Assert.Equal("Invalid userId or bookId", exception.Message);
        }
    }

    [Fact]
    public void ReturnBook_ReturnOnTime_NoFeesApplied()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_ReturnOnTime")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User
            {
                Id = 3,
                Name = "John Doe",
                FirstName = "John",
                Fees = 0m
            });

            context.Books.Add(new Book
            {
                Id = "c",
                Title = "Test Book",
                BorrowingDays = 14,
                FeePrice = 5m,
                Available = false
            });

            context.LibraryTransactions.Add(new LibraryTransaction
            {
                UserId = 3,
                BookId = "c",
                TransactionType = "Checkout",
                Date = DateTime.Now.AddDays(-14)
            });

            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.ReturnBook(3, "c");

            // Assert
            Assert.True(result);

            var user = context.Users.First(u => u.Id == 3);
            Assert.Equal(0m, user.Fees);

            var book = context.Books.First(b => b.Id == "c");
            Assert.True(book.Available);
        }
    }

    [Fact]
    public void ReturnBook_ReturnLate_FeesApplied()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_ReturnLate")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User
            {
                Id = 4,
                Name = "John Doe",
                FirstName = "John",
                Fees = 0m
            });

            context.Books.Add(new Book
            {
                Id = "d",
                Title = "Test Book",
                BorrowingDays = 14,
                FeePrice = 2.5m,
                Available = false
            });

            context.LibraryTransactions.Add(new LibraryTransaction
            {
                UserId = 4,
                BookId = "d",
                TransactionType = "Checkout",
                Date = DateTime.Now.AddDays(-16)
            });

            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.ReturnBook(4, "d");

            // Assert
            Assert.True(result);

            var user = context.Users.First(u => u.Id == 4);
            Assert.Equal(5m, user.Fees);

            var book = context.Books.First(b => b.Id == "d");
            Assert.True(book.Available);
        }
    }
    [Fact]
    public void ReturnBook_BookAlreadyAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_BookAlreadyAvailable")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 7, Name = "John", FirstName = "Doe", Fees = 0 });
            context.Books.Add(new Book { Id = "g", Title = "Test Book", Available = true, FeePrice = 2.5m, BorrowingDays = 14 });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => libraryService.ReturnBook(7, "g"));
            Assert.Equal("The book cannot be returned because it was not checked out by the current user.", exception.Message);
        }
    }
    [Fact]
    public void ReturnBook_NoValidCheckoutRecord_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_NoValidCheckoutRecord")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 8, Name = "John", FirstName = "Doe", Fees = 0 });

            context.Books.Add(new Book { Id = "h", Title = "Test Book", Available = false, FeePrice = 2.5m, BorrowingDays = 14 });

            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => libraryService.ReturnBook(8, "h"));
            Assert.Equal("No valid checkout record found for this book and user.", exception.Message);
        }
    }

    [Fact]
    public void ReturnBook_UserRenewsAndReturnsOnTime_NoLateFees()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_RenewAndReturnOnTime")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe", Fees = 0 });
            context.Books.Add(new Book { Id = "123", Title = "Test Book", BorrowingDays = 14, FeePrice = 2.5m, Available = false });

            context.LibraryTransactions.Add(new LibraryTransaction
            {
                UserId = 1,
                BookId = "123",
                TransactionType = "Checkout",
                Date = DateTime.Now.AddDays(-28) // Checked out 14 days ago
            });

            // Add renewal transaction
            context.LibraryTransactions.Add(new LibraryTransaction
            {
                UserId = 1,
                BookId = "123",
                TransactionType = "Renew",
                Date = DateTime.Now.AddDays(-14) 
            });

            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.ReturnBook(1, "123");

            // Assert
            Assert.True(result);

            var user = context.Users.FirstOrDefault(u => u.Id == 1);
            Assert.NotNull(user);
            Assert.Equal(0m, user.Fees);
        }
    }

    [Fact]
    public void ReturnBook_UserRenewsAndReturnsLate_AccruesLateFees()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_RenewAndReturnLate")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe", Fees = 0 });
            context.Books.Add(new Book { Id = "123", Title = "Test Book", BorrowingDays = 14, FeePrice = 2.5m, Available = false });
            
            context.LibraryTransactions.Add(new LibraryTransaction
            {
                UserId = 1,
                BookId = "123",
                TransactionType = "Checkout",
                Date = DateTime.Now.AddDays(-42)
            });

            context.LibraryTransactions.Add(new LibraryTransaction
            {
                UserId = 1,
                BookId = "123",
                TransactionType = "Renew",
                Date = DateTime.Now.AddDays(-28)
            });

            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.ReturnBook(1, "123");

            // Assert
            Assert.True(result);

            var user = context.Users.FirstOrDefault(u => u.Id == 1);
            Assert.NotNull(user);
            Assert.True(user.Fees > 0); // Late fees applied
            Assert.Equal(14 * 2.5m, user.Fees); // 14 overdue days * FeePrice = 35
        }
    }



    [Fact]
    public void GetUserLibraryTransactions_UserHasTransactions_ReturnsTransactionList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserHasTransactions")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe", Fees = 0 });
            context.LibraryTransactions.AddRange(
                new LibraryTransaction { Id = 1, UserId = 1, BookId = "123", TransactionType = "Checkout", Date = DateTime.Now.AddDays(-5) },
                new LibraryTransaction { Id = 2, UserId = 1, BookId = "456", TransactionType = "Return", Date = DateTime.Now.AddDays(-2) }
            );
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var transactions = libraryService.GetUserLibraryTransactions(1);

            // Assert
            Assert.NotNull(transactions);
            Assert.Equal(2, transactions.Count);
            Assert.True(transactions[0].Date > transactions[1].Date);
        }
    }

    [Fact]
    public void GetUserLibraryTransactions_UserHasNoTransactions_ReturnsEmptyList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserNoTransactions")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe", Fees = 0 });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var transactions = libraryService.GetUserLibraryTransactions(1);

            // Assert
            Assert.NotNull(transactions);
            Assert.Empty(transactions);
        }
    }

    [Fact]
    public void GetUserLibraryTransactions_UserDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserNotFound")
            .Options;

        using (var context = new ProjectContext(options))
        {
            // No users added
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => libraryService.GetUserLibraryTransactions(1));
            Assert.Equal("User not found.", exception.Message);
        }
    }

    [Fact]
    public void ProcessFeePayment_ValidPayment_ClearsFees()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_ProcessPayment")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User
            {
                Id = 1,
                Name = "John",
                FirstName = "Doe",
                Fees = 25.00m
            });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.ProcessFeePayment(1, 25.00m);

            // Assert
            Assert.Equal("Payment processed successfully. Outstanding fees have been cleared.", result);

            var user = context.Users.FirstOrDefault(u => u.Id == 1);
            Assert.NotNull(user);
            Assert.Equal(0m, user.Fees);
        }
    }

    [Fact]
    public void ProcessFeePayment_UserDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserNotFound")
            .Options;

        using (var context = new ProjectContext(options))
        {
            // No users added
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => libraryService.ProcessFeePayment(999, 25.00m));
            Assert.Equal("User not found.", exception.Message);
        }
    }
    [Fact]
    public void ProcessFeePayment_InvalidPaymentAmount_ReturnsErrorMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_InvalidPaymentAmount")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User
            {
                Id = 1,
                Name = "John",
                FirstName = "Doe",
                Fees = 25.00m
            });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.ProcessFeePayment(1, 20.00m);

            // Assert
            Assert.Equal("Payment failed: Payment amount does not match the outstanding fees. Outstanding Fees: 25,00", result);

            var user = context.Users.FirstOrDefault(u => u.Id == 1);
            Assert.NotNull(user);
            Assert.Equal(25.00m, user.Fees);
        }
    }

    [Fact]
    public void RenewBook_ValidRenewal_RenewsSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_ValidRenewal")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe" });
            context.Books.Add(new Book { Id = "123", Title = "Test Book", BorrowingDays = 14 });
            context.LibraryTransactions.Add(new LibraryTransaction
            {
                UserId = 1,
                BookId = "123",
                TransactionType = "Checkout",
                Date = DateTime.Now.AddDays(-5)
            });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.RenewBook(1, "123");

            // Assert
            Assert.Equal("Book renewed successfully. New due date: " + DateTime.Now.AddDays(23).ToShortDateString(), result);
        }
    }

    [Fact]
    public void RenewBook_UserDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserNotFound")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Books.Add(new Book { Id = "123", Title = "Test Book", BorrowingDays = 14 });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => libraryService.RenewBook(1, "123"));
            Assert.Equal("Invalid userId or bookId", exception.Message);
        }
    }

    [Fact]
    public void RenewBook_BookDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_BookNotFound")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe" });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => libraryService.RenewBook(1, "123"));
            Assert.Equal("Invalid userId or bookId", exception.Message);
        }
    }
    [Fact]
    public void RenewBook_NoCheckoutRecord_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_NoCheckoutRecord")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe" });
            context.Books.Add(new Book { Id = "123", Title = "Test Book", BorrowingDays = 14 });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => libraryService.RenewBook(1, "123"));
            Assert.Equal("No valid checkout record found for this book and user.", exception.Message);
        }
    }
    [Fact]
    public void RenewBook_AlreadyRenewed_ReturnsRenewalFailedMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_AlreadyRenewed")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe" });
            context.Books.Add(new Book { Id = "123", Title = "Test Book", BorrowingDays = 14 });
            context.LibraryTransactions.AddRange(
                new LibraryTransaction
                {
                    UserId = 1,
                    BookId = "123",
                    TransactionType = "Checkout",
                    Date = DateTime.Now.AddDays(-5)
                },
                new LibraryTransaction
                {
                    UserId = 1,
                    BookId = "123",
                    TransactionType = "Renew",
                    Date = DateTime.Now.AddDays(-1)
                }
            );
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);

            // Act
            var result = libraryService.RenewBook(1, "123");

            // Assert
            Assert.Equal("Renewal failed: Book has already been renewed.", result);
        }
    }

    [Fact]
    public void CheckOutBooks_AllBooksAvailable_ReturnsSuccessForAll()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_AllBooksAvailable")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe" });
            context.Books.AddRange(
                new Book { Id = "123", Title = "Book 1", Available = true },
                new Book { Id = "456", Title = "Book 2", Available = true }
            );
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);
            var bookIds = new List<string> { "123", "456" };

            // Act
            var results = libraryService.CheckOutBooks(1, bookIds);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Equal("Checkout successful.", results["123"]);
            Assert.Equal("Checkout successful.", results["456"]);
        }
    }

    [Fact]
    public void CheckOutBooks_SomeBooksUnavailable_ReturnsPartialSuccess()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_SomeBooksUnavailable")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe" });
            context.Books.AddRange(
                new Book { Id = "123", Title = "Book 1", Available = true },
                new Book { Id = "456", Title = "Book 2", Available = false }
            );
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);
            var bookIds = new List<string> { "123", "456" };

            // Act
            var results = libraryService.CheckOutBooks(1, bookIds);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Equal("Checkout successful.", results["123"]);
            Assert.Equal("Book not available", results["456"]);
        }
    }

    [Fact]
    public void CheckOutBooks_BookDoesNotExist_ReturnsErrorForMissingBooks()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_BookDoesNotExist")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe" });
            context.Books.Add(new Book { Id = "123", Title = "Book 1", Available = true });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);
            var bookIds = new List<string> { "123", "999" };

            // Act
            var results = libraryService.CheckOutBooks(1, bookIds);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Equal("Checkout successful.", results["123"]);
            Assert.Equal("Invalid userId or bookId", results["999"]);
        }
    }

    [Fact]
    public void CheckOutBooks_UserDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_UserDoesNotExist")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Books.Add(new Book { Id = "i", Title = "Book 1", Available = true });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);
            var bookIds = new List<string> { "i" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => libraryService.CheckOutBooks(999, bookIds));
            Assert.Equal("User not found.", exception.Message);
        }
    }
    [Fact]
    public void CheckOutBooks_EmptyBookList_ReturnsEmptyResults()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_EmptyBookList")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe" });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);
            var bookIds = new List<string>();

            // Act
            var results = libraryService.CheckOutBooks(1, bookIds);

            // Assert
            Assert.Empty(results);
        }
    }

    [Fact]
    public void CheckOutBooks_BookUnavailable_HandlesInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProjectContext>()
            .UseInMemoryDatabase(databaseName: "LibraryTestDb_BookUnavailable")
            .Options;

        using (var context = new ProjectContext(options))
        {
            context.Users.Add(new User { Id = 1, Name = "John", FirstName = "Doe" });
            context.Books.Add(new Book { Id = "123", Title = "Book 1", Available = false });
            context.SaveChanges();
        }

        using (var context = new ProjectContext(options))
        {
            var libraryService = new LibraryService(context);
            var bookIds = new List<string> { "123" };

            // Act
            var results = libraryService.CheckOutBooks(1, bookIds);

            // Assert
            Assert.Single(results);
            Assert.Equal("Book not available", results["123"]);
        }
    }
}
