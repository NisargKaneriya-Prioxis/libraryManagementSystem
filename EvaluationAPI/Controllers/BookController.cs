using EA.Model.RequestModel;
using EA.Model.ResponseModel;
using EA.Services.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EvaluationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController : BaseController
{
    private readonly IBookRepository _BookRepository;
    private readonly ILogger<BookController> _logger;

    public BookController(IBookRepository BookRepository, ILogger<BookController> logger)
    {
        _BookRepository = BookRepository;
        _logger = logger;
    }

    // GET ALL BOOKS - Dynamic
    [HttpGet("dynamicgetall")]
    public async Task<ActionResult<IEnumerable<BookResponseModel>>> GetallBooks([FromQuery] SearchRequestModel model)
    {
        _logger.LogInformation("Fetching all books with search params: {@SearchRequest}", model);

        var parameters = FillParamesFromModel(model);
        var list = await _BookRepository.List(parameters);

        if (list != null)
        {
            var result = JsonConvert.DeserializeObject<List<BookResponseModel>>(list.Result?.ToString() ?? "[]") ?? [];
            _logger.LogInformation("Retrieved {Count} books", result.Count);
            return Ok(result);
        }

        _logger.LogWarning("No books found matching search parameters");
        return NoContent();
    }

    // GET BY SID - Dynamic
    [HttpGet("dynamic/{booksid}")]
    public async Task<ActionResult<BookResponseModel>> GetByBookSID([FromRoute] string booksid)
    {
        _logger.LogInformation("Fetching book with SID: {Sid}", booksid);

        var book = await _BookRepository.GetbookBySid(booksid);

        if (book == null)
        {
            _logger.LogWarning("Book not found with SID: {Sid}", booksid);
            return NotFound(new { message = $"Book with SID '{booksid}' not found" });
        }

        _logger.LogInformation("Successfully retrieved book: {Title} (SID: {Sid})", book.Title, booksid);
        return Ok(book);
    }

    // Insert
    [HttpPost("InsertBook")]
    public async Task<ActionResult<List<BookResponseModel>>> InsertBook([FromBody] List<BookRequestWithouSidModel> book)
    {
        List<BookResponseModel> createdBook = await _BookRepository.InsertBook(book);
        if (createdBook == null)
        {
            _logger.LogInformation("Failed to create book: {@BookData}", book);
            return BadRequest();
        }

        _logger.LogInformation("Book created successfully");
        return Ok(createdBook);
    }

    // Update
    [HttpPost("updateBook/{BookSID}")]
    public async Task<ActionResult<BookResponseModel>> UpdateBook([FromBody] BookRequestWithouSidModel model, [FromRoute] string booksid)
    {
        var book = await _BookRepository.UpdateBook(booksid, model);
        if (book != null)
        {
            _logger.LogInformation("Book with SID {BookSID} updated successfully.", booksid);
            return Ok(book);
        }

        _logger.LogInformation("Book with SID {BookSID} not found for update.", booksid);
        return NotFound();
    }

    // Borrowed book 
    [HttpPost("borrow/{bookSid}/{isbn}")]
    public async Task<IActionResult> BorrowBook(string bookSid, string isbn)
    {
        _logger.LogInformation("Borrow request received for Book SID: {Sid}, ISBN: {isbn}", bookSid, isbn);

        var result = await _BookRepository.BorrowedBook(bookSid, isbn);

        if (!result)
        {
            return BadRequest(new
            {
                Success = false,
                Message = $"Book with SID: {bookSid} and ISBN: {isbn} is already borrowed."
            });
        }

        return Ok(new
        {
            Success = true,
            Message = $"Book with SID: {bookSid} and ISBN: {isbn} successfully borrowed."
        });
    }

    // Return Book 
    [HttpPost("return/{bookSid}/{isbn}")]
    public async Task<IActionResult> ReturnBook(string bookSid, string isbn)
    {
        _logger.LogInformation("Return request received for Book SID: {Sid}, ISBN: {isbn}", bookSid, isbn);

        var result = await _BookRepository.ReturnBook(bookSid, isbn);

        if (!result)
        {
            return BadRequest(new
            {
                Success = false,
                Message = $"Book with SID: {bookSid} and ISBN: {isbn} is already marked as available."
            });
        }

        return Ok(new
        {
            Success = true,
            Message = $"Book with SID: {bookSid} and ISBN: {isbn} successfully returned."
        });
    }

    // DELETE
    [HttpDelete("dynamicdelete/{bookSid}")]
    public async Task<ActionResult> DeleteBook([FromRoute] string bookSid)
    {
        _logger.LogInformation("Request to delete book with SID: {Sid}", bookSid);

        var result = await _BookRepository.Deletebook(bookSid);

        if (!result)
        {
            _logger.LogWarning("Book not found for deletion. SID: {Sid}", bookSid);
            return NotFound($"Book with SID '{bookSid}' not found.");
        }

        _logger.LogInformation("Successfully deleted book with SID: {Sid}", bookSid);
        return Ok("Book deleted.");
    }
}
