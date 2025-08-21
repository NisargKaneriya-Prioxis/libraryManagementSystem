using System.Net;
using AutoMapper;
using EA.Common;
using EA.Model.CommonModel;
using EA.Model.Models.MyLibraryDB;
using EA.Model.RequestModel;
using EA.Model.ResponseModel;
using EA.Model.SpDbContext;
using EA.Services.Repositories.Interface;
using EA.Services.RepositoryFactory;
using EA.Services.UnitOfWork;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EA.Services.Repositories.Implementation;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<BookRepository> _logger;
    private readonly LibraryManagementSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;

    public BookRepository(
        LibraryDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<BookRepository> logger,
        LibraryManagementSpContext spContext)
    {
        _context = context;
        _logger = logger;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Page> List(Dictionary<string, object> parameters)
    {
        _logger.LogInformation("Fetching list of books with parameters: {@Params}", parameters);

        try
        {
            var xmlParam = CommonHelper.DictionaryToXml(parameters, "Search");
            string sqlQuery = "SP_GetAllBookdynamic {0}";
            object[] param = { xmlParam };

            var res = await _spContext.ExecutreStoreProcedureResultList(sqlQuery, param);

            var list = JsonConvert.DeserializeObject<List<BookResponseModel>>(res.Result?.ToString() ?? "[]");
            

            if (list == null || !list.Any())
            {
                _logger.LogWarning("No books found with the given parameters: {@Params}", parameters);
                throw new HttpStatusCodeException(404, "No books found");
            }

            _logger.LogInformation("Books list retrieved successfully.");
            return res;
        }
        catch (HttpStatusCodeException ex)
        {
            _logger.LogWarning("Known error occurred while fetching books list: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching books list with parameters: {@Params}", parameters);
            throw new HttpStatusCodeException(500, "An unexpected error occurred while fetching the book list");
        }
    }


    // Get By SID
    public async Task<BookResponseModel?> GetbookBySid(string booksid)
    {
        _logger.LogInformation("Fetching book with SID: {Sid}", booksid);

        try
        {
            string sqlQuery = "SP_GetBookById {0}";
            object[] param = { booksid };

            var jsonResult = await _spContext.ExecuteStoreProcedure(sqlQuery, param);

            if (string.IsNullOrEmpty(jsonResult))
            {
                _logger.LogWarning("No book found with SID: {Sid}", booksid);
                throw new HttpStatusCodeException(404, $"Book with SID '{booksid}' not found");
            }

            var book = JsonConvert.DeserializeObject<BookResponseModel>(jsonResult);

            if (book == null)
            {
                _logger.LogWarning("Deserialization failed or empty result for book SID: {Sid}", booksid);
                throw new HttpStatusCodeException(500, "Failed to parse book details");
            }

            _logger.LogInformation("Successfully retrieved book: {Title} (SID: {Sid})", book.Title, booksid);
            return book;
        }
        catch (HttpStatusCodeException ex)
        {
            _logger.LogWarning("Known error occurred while fetching book with SID {Sid}: {Message}", booksid, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching book with SID: {Sid}", booksid);
            throw new HttpStatusCodeException(500, "An unexpected error occurred while fetching the book");
        }
    }
    public async Task<List<BookResponseModel>> InsertBook(List<BookRequestWithouSidModel> books)
{
    _logger.LogInformation("Inserting {Count} new books", books?.Count ?? 0);

    try
    {
        if (books == null || books.Count == 0)
        {
            _logger.LogWarning("Insert failed: Book list cannot be empty.");
            throw new HttpStatusCodeException(400, "Book list cannot be empty.");
        }

        List<Book> bookList = new List<Book>();

        foreach (var book in books)
        {
            if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author))
            {
                _logger.LogWarning("Insert failed: Missing Title/Author for book request {@Book}", book);
                throw new HttpStatusCodeException(400, "Book Title and Author are required.");
            }
            var existingTitle = await _unitOfWork.GetRepository<Book>()
                .GetAllAsync(b => b.Title == book.Title);
            
            if (existingTitle.Any())
            {
                _logger.LogWarning("Insert failed: Duplicate Title detected - {Title}", book.Title);
                throw new HttpStatusCodeException(409, $"A book with the title '{book.Title}' already exists.");
            }
            
            if (!string.IsNullOrWhiteSpace(book.Isbn))
            {
                var existingIsbn = await _unitOfWork.GetRepository<Book>()
                    .GetAllAsync(b => b.Isbn == book.Isbn);
            
                if (existingIsbn.Any())
                {
                    _logger.LogWarning("Insert failed: Duplicate ISBN detected - {Isbn}", book.Isbn);
                    throw new HttpStatusCodeException(409, $"A book with the ISBN '{book.Isbn}' already exists.");
                }
            }

            var b = new Book
            {
                BookSid = "LIB" + Guid.NewGuid(),
                Author = book.Author,
                Title = book.Title,
                PublishedYear = book.PublishedYear,
                Isbn = book.Isbn,
                BorrowedStatus = (int)IsAvailable.Available
            };

            bookList.Add(b);
        }

        await _unitOfWork.GetRepository<Book>().InsertAsync(bookList);
        await _unitOfWork.CommitAsync();

        var resBooks = bookList.Select(b => new BookResponseModel
        {
            BookSid = b.BookSid,
            Author = b.Author,
            Title = b.Title,
            PublishedYear = b.PublishedYear,
            Isbn = b.Isbn,
            BorrowedStatus = b.BorrowedStatus
        }).ToList();

        _logger.LogInformation("Successfully inserted {Count} books", resBooks.Count);
        return resBooks;
    }
    catch (HttpStatusCodeException ex)
    {
        _logger.LogWarning("Known error occurred while inserting books: {Message}", ex.Message);
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error occurred while inserting books");
        throw new HttpStatusCodeException(500, "An unexpected error occurred while inserting books");
    }
}
    public async Task<BookResponseModel?> UpdateBook(string bookSid, BookRequestWithouSidModel book)
    {
        _logger.LogInformation("Updating book with SID: {Sid}", bookSid);

        try
        {
            var existingBook = await _unitOfWork.GetRepository<Book>()
                .SingleOrDefaultAsync(x =>
                    x.BookSid == bookSid &&
                    x.BorrowedStatus == (int)IsAvailable.Available &&
                    x.Status == (int)Status.NotDeleted);

            if (existingBook == null)
            {
                _logger.LogWarning("Book not found or unavailable for update. SID: {Sid}", bookSid);
                throw new HttpStatusCodeException(404, $"Book with SID '{bookSid}' not found or unavailable for update.");
            }

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.Isbn = book.Isbn;
            existingBook.PublishedYear = book.PublishedYear;
            existingBook.BorrowedStatus = (int)IsAvailable.Available;
            existingBook.Status = (int)Status.NotDeleted;

            _unitOfWork.GetRepository<Book>().Update(existingBook);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Book updated successfully with SID: {Sid}", bookSid);

            return new BookResponseModel
            {
                BookSid = existingBook.BookSid,
                Title = existingBook.Title,
                Author = existingBook.Author,
                Isbn = existingBook.Isbn,
                PublishedYear = existingBook.PublishedYear,
                BorrowedStatus = existingBook.BorrowedStatus,
                Status = existingBook.Status
            };
        }
        catch (HttpStatusCodeException) // already custom handled
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error occurred while updating book with SID: {Sid}", bookSid);
            throw new HttpStatusCodeException(500, "An unexpected error occurred while updating books");
        }
    }

    
    //BorrowedBook 
    public async Task<bool> BorrowedBook(string bookSid, string Isbn)
    {
        _logger.LogInformation("Borrowing book with SID: {Sid} and ISBN: {isbn}", bookSid, Isbn);

        try
        {
            var books = await _unitOfWork.GetRepository<Book>().GetAllAsync();

            var book = books.FirstOrDefault(x =>
                x.BookSid == bookSid &&
                x.Isbn == Isbn &&
                x.BorrowedStatus == (int)IsAvailable.Available &&
                x.Status == (int)Status.NotDeleted);

            if (book == null)
            {
                _logger.LogWarning("Book with SID: {Sid} and ISBN: {isbn} not available or already borrowed", bookSid, Isbn);
                throw new HttpStatusCodeException(404, $"Book with SID '{bookSid}' and ISBN '{Isbn}' not available or already borrowed.");
            }

            book.BorrowedStatus = (int)IsAvailable.NotAvailable;

            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book with SID: {Sid} and ISBN: {isbn} successfully borrowed", bookSid, Isbn);
            return true;
        }
        catch (HttpStatusCodeException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred while borrowing book with SID: {Sid} and ISBN: {isbn}", bookSid, Isbn);
            throw new HttpStatusCodeException(500, "An unexpected error occurred while borrowing the book. Please try again later.");
        }
    }

    //Bookreturn 
    public async Task<bool> ReturnBook(string bookSid, string Isbn)
    {
        _logger.LogInformation("Return book with SID: {Sid} and ISBN: {isbn}", bookSid, Isbn);

        try
        {
            var books = await _unitOfWork.GetRepository<Book>().GetAllAsync();
            var book = books.FirstOrDefault(x =>
                x.BookSid == bookSid && 
                x.Isbn == Isbn && 
                x.BorrowedStatus == (int)IsAvailable.NotAvailable &&
                x.Status == (int)Status.NotDeleted);

            if (book == null)
            {
                _logger.LogWarning("Book with SID: {Sid} and ISBN: {isbn} not found or already available in the library", bookSid, Isbn);
                throw new HttpStatusCodeException(404, $"Book with SID: {bookSid} and ISBN: {Isbn} not found or already returned.");
            }

            book.BorrowedStatus = (int)IsAvailable.Available;

            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book with SID: {Sid} and ISBN: {isbn} successfully returned", bookSid, Isbn);
            return true;
        }
        catch (HttpStatusCodeException) 
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred while returning book with SID: {Sid} and ISBN: {isbn}", bookSid, Isbn);
            throw new HttpStatusCodeException(500, "An unexpected error occurred while returning the book.");
        }
    }


    // Delete Book (Mark as Not Available)
    public async Task<bool> Deletebook(string bookSid)
    {
        _logger.LogInformation("Deleting (marking as unavailable) book with SID: {Sid}", bookSid);
        try
        {
            var books = await _unitOfWork.GetRepository<Book>().GetAllAsync();
            var book = books.FirstOrDefault(x =>
                x.BookSid == bookSid && x.BorrowedStatus == (int)IsAvailable.Available &&
                x.Status == (int)Status.NotDeleted);

            if (book == null)
            {

                _logger.LogWarning("Book not found for deletion. SID: {Sid}", bookSid);
                throw new HttpStatusCodeException(400, "Book not found for deletion");
            }

            book.Status = (int)Status.Deleted;

            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book successfully marked as unavailable. SID: {Sid}", bookSid);
            return true;
        }
        catch (HttpStatusCodeException ex)
        {
            _logger.LogWarning("Deleting failed Book with the Sid:{Sid}", bookSid);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while deleting (marking unavailable) book with SID: {Sid}", bookSid);
            throw;
        }
    }






}
