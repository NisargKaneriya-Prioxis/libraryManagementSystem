using FluentValidation;
using EA.Model.RequestModel;

public class BookRequestModelValidator : AbstractValidator<BookRequestModel>
{
    public BookRequestModelValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(50).WithMessage("Title must not exceed 50 characters.");

        RuleFor(x => x.BookSid)
            .NotEmpty().WithMessage("Book SID is required.")
            .MaximumLength(50).WithMessage("Book SID must not exceed 50 characters.");

        RuleFor(x => x.Author)
            .MaximumLength(50).WithMessage("Author name must not exceed 50 characters.");

        RuleFor(x => x.Isbn)
            .MaximumLength(50).WithMessage("ISBN must not exceed 50 characters.");

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1000, 2100).When(x => x.PublishedYear.HasValue)
            .WithMessage("Published year must be between 1000 and 2100.");

        RuleFor(x => x.BorrowedStatus)
            .Must(status => status == 1 || status == 2)
            .WithMessage("BorrowedStatus must be either 1 (Available) or 2 (Borrowed).");

        RuleFor(x => x.Status)
            .Must(status => status == 1 || status == 2 || status == 3 || status == 4)
            .WithMessage("Status must be between 1 and 4.");
    }
}