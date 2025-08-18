namespace EA.Model.SpDbContext;

    /// <summary>
    /// Represents the result of executing a stored procedure.
    /// </summary>
    public class ExecutreStoreProcedureResult
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public string? Result { get; set; }
    }

    /// <summary>
    /// Represents the result of executing a stored procedure with an entity SID.
    /// </summary>
    public class ExecutreStoreProcedureResultWithEntitySID
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// Gets or sets the entity SID.
        /// </summary>
        public string? EntitiySID { get; set; }
    }

    /// <summary>
    /// Represents the result of executing a stored procedure with an SID.
    /// </summary>
    public class ExecutreStoreProcedureResultWithSID
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the SID.
        /// </summary>
        public string? SID { get; set; }
    }

    /// <summary>
    /// Represents the result of executing a stored procedure with an ID.
    /// </summary>
    public class ExecuteStoreProcedureResultWithId
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// Represents the result of executing a stored procedure with a list of results.
    /// </summary>
    public class ExecutreStoreProcedureResultList
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int TotalCount { get; set; }
    }