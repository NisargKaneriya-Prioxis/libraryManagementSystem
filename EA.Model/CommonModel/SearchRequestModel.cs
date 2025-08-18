using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class SearchRequestModel
{
    /// <summary>
    /// 
    /// </summary>
    public SearchRequestModel()
    {
        Page = 1;
        PageSize = 10;
    }

    /// <summary>
    /// Search string to look up for matching results. 
    /// </summary>
    [JsonPropertyName("search_text")]
    [MaxLength(100, ErrorMessage = "Search text cannot exceed 100 characters.")]
    public string? SearchText { get; set; }

    /// <summary>
    /// Expected page number in the result set.
    /// </summary>
    [JsonPropertyName("page")]

    public int Page { get; set; }

    /// <summary>
    /// Page size of the result set.
    /// </summary>
    [JsonPropertyName("page_size")]


    public int PageSize { get; set; }

    /// <summary>
    /// The column / attribute by which the results shall be sorted.
    /// </summary>
    [JsonPropertyName("sort_column")]
    public string? SortColumn { get; set; }

    /// <summary>
    /// The order by which the results shall be sorted.  Possible values are 'asc' for ascending order, 'desc' for descending order.
    /// </summary>
    [JsonPropertyName("sort_order")]

    public string? SortOrder { get; set; }

    /// <summary>
    /// Search filter list to look up for matching results. If must be in format '[{key:'keyname',value:'keyvalue'},{key:'keyname',value:'keyvalue'}]'.
    /// </summary>
    public string? Filters { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public class FilterRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("value")]
        public object Value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("from")]
        public object From { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("to")]
        public object To { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class DateRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }
    }
}