using DAL.Models;
using DAL;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibraryProject
{
    public class GeneralScenarioTests
    {
        private readonly Mock<ProjectContext> _mockContext;
        private readonly Mock<DbSet<User>> _mockUserDbSet;
        private readonly Mock<DbSet<Book>> _mockBookDbSet;

        public GeneralScenarioTests()
        {
            _mockContext = new Mock<ProjectContext>();
            _mockUserDbSet = new Mock<DbSet<User>>();
            _mockBookDbSet = new Mock<DbSet<Book>>();
        }

        [Fact]
        public void UserExists_ReturnsTrue_WhenUserExists()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Name = "Doe", FirstName = "John", Fees = 0 }
            }.AsQueryable();

            _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
            _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
            _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
            _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            _mockContext.Setup(c => c.Users).Returns(_mockUserDbSet.Object);

            // Act
            var exists = _mockContext.Object.Users.Any(u => u.Id == 1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void BookExists_ReturnsTrue_WhenBookExists()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Test Book", Available = true }
            }.AsQueryable();

            _mockBookDbSet.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(books.Provider);
            _mockBookDbSet.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(books.Expression);
            _mockBookDbSet.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(books.ElementType);
            _mockBookDbSet.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(books.GetEnumerator());

            _mockContext.Setup(c => c.Books).Returns(_mockBookDbSet.Object);

            // Act
            var exists = _mockContext.Object.Books.Any(b => b.Id == 1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void BookAvailable_ReturnsTrue_WhenBookIsAvailable()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Test Book", Available = true }
            }.AsQueryable();

            _mockBookDbSet.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(books.Provider);
            _mockBookDbSet.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(books.Expression);
            _mockBookDbSet.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(books.ElementType);
            _mockBookDbSet.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(books.GetEnumerator());

            _mockContext.Setup(c => c.Books).Returns(_mockBookDbSet.Object);

            // Act
            var isAvailable = _mockContext.Object.Books.FirstOrDefault(b => b.Id == 1)?.Available;

            // Assert
            Assert.True(isAvailable);
        }
    }
}
