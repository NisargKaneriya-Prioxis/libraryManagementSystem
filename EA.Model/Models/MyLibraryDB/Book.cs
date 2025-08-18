using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EA.Model.Models.MyLibraryDB;

[Table("Book")]
[Index("Title", Name = "UQ__Book__2CB664DC0C7EB697", IsUnique = true)]
[Index("Isbn", Name = "UQ__Book__447D36EA6EB62D88", IsUnique = true)]
[Index("BookSid", Name = "UQ__Book__EEC908F135845CCE", IsUnique = true)]
public partial class Book
{
    [Key]
    [Column("BookID")]
    public int BookId { get; set; }

    [StringLength(50)]
    public string? Title { get; set; }

    [Column("BookSID")]
    [StringLength(50)]
    public string BookSid { get; set; } = null!;

    [StringLength(50)]
    public string? Author { get; set; }

    [Column("ISBN")]
    [StringLength(50)]
    public string? Isbn { get; set; }

    public int? PublishedYear { get; set; }

    public int BorrowedStatus { get; set; }

    public int Status { get; set; }
}
