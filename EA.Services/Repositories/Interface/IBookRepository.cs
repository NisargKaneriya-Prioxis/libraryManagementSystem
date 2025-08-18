using EA.Model.CommonModel;
using EA.Model.RequestModel;
using EA.Model.ResponseModel;

namespace EA.Services.Repositories.Interface;

public interface IBookRepository
{
    Task<Page> List(Dictionary<string, object> parameters);

    Task<BookResponseModel?> GetbookBySid(string bookSid);
    
    // Task<BookResponseModel> InsertBook(BookRequestWithouSidModel Book);
    Task<List<BookResponseModel>> InsertBook(List<BookRequestWithouSidModel> books);

    Task<BookResponseModel> UpdateBook(String bookSid, BookRequestWithouSidModel Book);

    Task<bool> BorrowedBook(string bookSid, string Isbn);

    Task<bool> ReturnBook(string bookSid, string Isbn);

    Task<bool> Deletebook(string bookSid);
}