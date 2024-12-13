using Microsoft.AspNetCore.Mvc;
using WebAPILibrary.Services;

namespace WebAPILibrary.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LibraryController : ControllerBase
    {
        private readonly ILibraryService _libraryService;

        public LibraryController(ILibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        [HttpGet("fees/{userId}")]
        public IActionResult GetOutstandingFees(int userId)
        {
            try
            {
                var fees = _libraryService.GetOutstandingFees(userId);
                return Ok(new { UserId = userId, OutstandingFees = fees });
            }
            catch (ArgumentException ex) when (ex.Message == "User not found.")
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An internal error occurred. Please try again later." });
            }
        }

        [HttpPost("checkoutBook")]
        public IActionResult CheckOutBook(int userId, string bookId)
        {
            try
            {
                var result = _libraryService.CheckOutBook(userId, bookId);

                if (result)
                {
                    return Ok(new { Message = "Book checked out successfully." });
                }
                else
                {
                    return BadRequest(new { Message = "Book could not be checked out." });
                }
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An internal error occurred. Please try again later." });
            }
        }

        [HttpPost("return")]
        public IActionResult ReturnBook(int userId, string bookId)
        {
            try
            {
                var result = _libraryService.ReturnBook(userId, bookId);

                if (result)
                {
                    return Ok(new { Message = "Book returned successfully." });
                }
                else
                {
                    return BadRequest(new { Message = "Book could not be returned." });
                }
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An internal error occurred. Please try again later." });
            }
        }

        [HttpGet("transactions/{userId}")]
        public IActionResult GetUserLibraryTransactions(int userId)
        {
            try
            {
                var transactions = _libraryService.GetUserLibraryTransactions(userId);

                return Ok(new
                {
                    UserId = userId,
                    Transactions = transactions.Select(t => new
                    {
                        t.Id,
                        t.BookId,
                        t.TransactionType,
                        t.Date
                    })
                });
            }
            catch (ArgumentException ex) when (ex.Message == "User not found.")
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An internal error occurred. Please try again later." });
            }
        }

        [HttpPost("payment")]
        public IActionResult ProcessFeePayment(int userId, decimal paymentAmount)
        {
            try
            {
                var result = _libraryService.ProcessFeePayment(userId, paymentAmount);

                if (result.StartsWith("Payment processed successfully"))
                {
                    return Ok(new { Message = result });
                }
                else
                {
                    return BadRequest(new { Message = result });
                }
            }
            catch (ArgumentException ex) when (ex.Message == "User not found.")
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An internal error occurred. Please try again later." });
            }
        }

        [HttpPost("renew")]
        public IActionResult RenewBook(int userId, string bookId)
        {
            try
            {
                var result = _libraryService.RenewBook(userId, bookId);

                if (result.StartsWith("Renewal failed"))
                {
                    return BadRequest(new { Message = result });
                }

                return Ok(new { Message = result });
            }
            catch (ArgumentException ex) when (ex.Message == "User not found." || ex.Message == "Book not found.")
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An internal error occurred. Please try again later." });
            }
        }

        [HttpPost("checkoutBooks")]
        public IActionResult CheckOutBooks(int userId, [FromBody] List<string> bookIds)
        {
            try
            {
                // Call the service method to process the multiple check-outs
                var results = _libraryService.CheckOutBooks(userId, bookIds);

                // Return the results as a 200 OK response
                return Ok(new { UserId = userId, Results = results });
            }
            catch (ArgumentException ex) when (ex.Message == "User not found.")
            {
                // Handle user not found error
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                // Handle unexpected errors
                return StatusCode(500, new { Message = "An internal error occurred. Please try again later." });
            }
        }

    }
}
