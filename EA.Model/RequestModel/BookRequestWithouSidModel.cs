using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EA.Model.RequestModel;

public class BookRequestWithouSidModel
{
    [Key]
    [Column("BookID")]
    public int BookId { get; set; }

    [StringLength(50)]
    public string? Title { get; set; }

    // [Column("BookSID")]
    // [StringLength(50)]
    // public string BookSid { get; set; } = null!;

    [StringLength(50)]
    public string? Author { get; set; }

    [Column("ISBN")]
    [StringLength(50)]
    public string? Isbn { get; set; }

    public int? PublishedYear { get; set; }

    public int BorrowedStatus { get; set; }
    
    public int Status { get; set; }
}