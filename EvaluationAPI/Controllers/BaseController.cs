using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using EA.Common;
using Newtonsoft.Json;
using Constants = Microsoft.VisualBasic.Constants;

namespace EvaluationAPI.Controllers;

public class BaseController : ControllerBase
{
    #region Variables & Constructor
        protected static readonly HashSet<string> sqlReservedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SELECT", "INSERT", "DELETE", "UPDATE", "DROP", "CREATE", "ALTER", "WHERE",
                "FROM", "JOIN", "GROUP", "ORDER", "HAVING", "LIMIT", "OFFSET","CASE","TRUNCATE",
                "ROLLBACK", "SAVEPOINT","SHUTDOWN","RESTORE", "GRANT", "REVOKE" ,"RANDOMBLOB", "SLEEP"
            };
        public BaseController() { }
        #endregion

        #region Fill Parames From Model
        /// <summary>
        /// Fill params from model
        /// </summary>
        /// <param name="model">Request params</param>
        /// <param name="user_id">User ID</param>
        /// <returns>Dictionary object</returns>
        protected Dictionary<string, object> FillParamesFromModel(SearchRequestModel searchModel, long userID = 0)
        {
            var parameters = new Dictionary<string, object>();
            double pageStart = 1, pageSize = 10;

            if (searchModel != null)
            {
                if (!double.TryParse(searchModel.Page.ToString(), out pageStart) || pageStart <= 0)
                {
                    throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, "Invalid Page Start");
                }

                if (!double.TryParse(searchModel.PageSize.ToString(), out pageSize) || pageSize < -1 || pageSize > 10000)
                {
                    throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, "Invalid Page Size");
                }

                string sortColumn = !string.IsNullOrWhiteSpace(searchModel.SortColumn) ? searchModel.SortColumn.Trim() : "BookSid";
                if (sqlReservedWords.Contains(sortColumn))
                {
                    throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, $"Invalid Sort Column: '{sortColumn}' contains a SQL reserved word.");
                }
                parameters.Add(EA.Common.Constants.SearchParameters.SortColumn, sortColumn);

                string sortOrder = !string.IsNullOrWhiteSpace(searchModel.SortOrder) ? searchModel.SortOrder.Trim() : "DESC";
                if (sqlReservedWords.Contains(sortOrder))
                {
                    throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, $"Invalid Sort Order: '{sortOrder}' contains a SQL reserved word.");
                }
                parameters.Add(EA.Common.Constants.SearchParameters.SortOrder, sortOrder);

                string searchText = !string.IsNullOrWhiteSpace(searchModel.SearchText) ? ToEscapeXml(searchModel.SearchText.Trim()) : "%";
                if (sqlReservedWords.Contains(searchText))
                {
                    throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, $"Invalid Search Text: '{searchText}' contains a SQL reserved word.");
                }
                parameters.Add(EA.Common.Constants.SearchParameters.SearchText, searchText);

                if (!string.IsNullOrWhiteSpace(searchModel.Filters))
                {
                    string filter = "";
                    if (searchModel.Filters.IsValidJson())
                    {
                        filter = GetFilterConditionFromModel(searchModel.Filters.Trim());
                    }

                    parameters.Add(EA.Common.Constants.SearchParameters.Filters, filter.Trim());
                }
                else
                {
                    parameters.Add(EA.Common.Constants.SearchParameters.Filters, "1 = 1 AND");
                }
            }

            if (userID > 0)
            {
                parameters.Add("LoggedInUserID", userID);
            }

            parameters.Add(EA.Common.Constants.SearchParameters.PageStart, pageStart);
            parameters.Add(EA.Common.Constants.SearchParameters.PageSize, pageSize);

            return parameters;
        }

        #endregion

        #region Get FilterCondition From Model
        /// <summary>
        /// Converts a JSON string representing filter conditions into a SQL-like filter condition string.
        /// </summary>
        /// <param name="filter">The JSON string containing filter conditions.</param>
        /// <returns>A SQL-like filter condition string.</returns>
        /// <exception cref="HttpStatusCodeException">Thrown when an invalid filter condition is encountered.</exception>
        protected string GetFilterConditionFromModel(string filter)
        {
            string condition = "1 = 1 AND";
            if (string.IsNullOrWhiteSpace(filter))
            {
                return condition;
            }
            else
            {
                condition = " ";
            }

            var filterConditions = JsonConvert.DeserializeObject<List<SearchRequestModel.FilterRequestModel>>(filter);
            int i = 0;

            foreach (var item in filterConditions)
            {
                i++;
                // Check if `item.Key` contains reserved words
                if (!string.IsNullOrWhiteSpace(item.Key) && sqlReservedWords.Contains(item.Key))
                {
                    throw new HttpStatusCodeException(StatusCodes.Status400BadRequest,
                        $"Invalid Filter Key: '{item.Key}' contains a SQL reserved word.");
                }

                if (item.Value != null)
                {
                    item.Value = ToEscapeXml(item.Value.ToString());
                    if (sqlReservedWords.Contains(item.Value.ToString()))
                    {
                        throw new HttpStatusCodeException(StatusCodes.Status400BadRequest,
                            $"Invalid Filter Value: '{item.Value}' contains a SQL reserved word.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(item.Condition))
                {
                    string query = GenerateConditionQuery(item);
                    condition += query + " AND ";
                }
                else
                {
                    condition += GenerateLikeQuery(item) + " AND ";
                }
            }

            return condition;
        }

        /// <summary>
        /// Generates the SQL condition query for the given filter item.
        /// </summary>
        /// <param name="item">The filter item.</param>
        /// <returns>The SQL condition query.</returns>
        /// <exception cref="HttpStatusCodeException">Thrown when an invalid filter condition is encountered.</exception>
        private string GenerateConditionQuery(SearchRequestModel.FilterRequestModel item)
        {
            string condition = item.Condition.ToLower();
            switch (condition)
            {
                case "in":
                    return GenerateInQuery(item, condition == "in");
                case "nin":
                    return GenerateInQuery(item, condition == "in");
                case "between":
                    return GenerateBetweenQuery(item);
                case "=":
                    return GenerateComparisonQuery(item, condition);
                case ">=":
                    return GenerateComparisonQuery(item, condition);
                case ">":
                    return GenerateComparisonQuery(item, condition);
                case "<=":
                    return GenerateComparisonQuery(item, condition);
                case "<":
                    return GenerateComparisonQuery(item, condition);
                default:
                    throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, "Invalid Filter");
            }
        }

        /// <summary>
        /// Generates the SQL IN or NOT IN query for the given filter item.
        /// </summary>
        /// <param name="item">The filter item.</param>
        /// <param name="isIn">Indicates whether it is an IN query or NOT IN query.</param>
        /// <returns>The SQL IN or NOT IN query.</returns>
        private string GenerateInQuery(SearchRequestModel.FilterRequestModel item, bool isIn)
        {
            var data = item.Value.ToString().Split(",");
            string query = string.Join(", ", data.Select(val =>
            {
                return int.TryParse(val, out int intValue) ? intValue.ToString() : $"'{val}'";
            }));

            return $"{item.Key} {(isIn ? "IN" : "NOT IN")} ({query})";
        }

        /// <summary>
        /// Generates the SQL BETWEEN query for the given filter item.
        /// </summary>
        /// <param name="item">The filter item.</param>
        /// <returns>The SQL BETWEEN query.</returns>
        private string GenerateBetweenQuery(SearchRequestModel.FilterRequestModel item)
        {
            if (int.TryParse(item.From.ToString(), out int _))
            {
                return $"{item.Key} BETWEEN {item.From} AND {item.To}";
            }

            try
            {
                DateTime fromDate;
                DateTime toDate;
                bool isFromDateValid = DateTime.TryParse(item.From.ToString(), out fromDate);
                bool isToDateValid = DateTime.TryParse(item.To.ToString(), out toDate);

                if (isFromDateValid && isToDateValid)
                {
                    toDate = toDate.Date.AddDays(1).AddTicks(-1); // Last moment of the day

                    // Format dates to SQL-friendly string
                    item.From = fromDate.ToString("yyyy-MM-dd HH:mm:ss");
                    item.To = toDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex)
            {
                // Handle parsing or other errors if needed
            }

            return $"{item.Key} BETWEEN '{item.From}' AND '{item.To}'";
        }

        /// <summary>
        /// Generates the SQL comparison query for the given filter item.
        /// </summary>
        /// <param name="item">The filter item.</param>
        /// <param name="condition">The comparison condition.</param>
        /// <returns>The SQL comparison query.</returns>
        private string GenerateComparisonQuery(SearchRequestModel.FilterRequestModel item, string condition)
        {
            var data = item.Key.ToString().Split(",");
            string query = string.Join(" OR ", data.Select(keyVal =>
            {
                int integer;
                DateTime date;

                bool isIntegerValid = int.TryParse(item.Value.ToString(), out integer);
                bool isDateValid = DateTime.TryParseExact(item.Value.ToString(), "dd-M-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                return isIntegerValid
                ? $"{keyVal} {condition} {integer}"
                : isDateValid
                    ? $"{keyVal} {condition} '{date:yyyy-MM-dd HH:mm:ss}'"
                    : $"{keyVal} {condition} '{item.Value}'";
            }));

            return $"( {query} )";
        }

        /// <summary>
        /// Generates the SQL LIKE query for the given filter item.
        /// </summary>
        /// <param name="item">The filter item.</param>
        /// <returns>The SQL LIKE query.</returns>
        private string GenerateLikeQuery(SearchRequestModel.FilterRequestModel item)
        {
            var data = item.Key.ToString().Split(",");
            string query = string.Join(" OR ", data.Select(keyVal =>
            {
                return int.TryParse(item.Value.ToString(), out int intValue)
                    ? $"{keyVal} = {intValue}"
                    : $"{keyVal} LIKE ('%{item.Value}%')";
            }));

            return $"( {query} )";
        }

        #endregion

        #region To Escape Xml
        /// <summary>
        /// Escapes special characters in a string for XML.
        /// </summary>
        /// <param name="s">The input string to escape.</param>
        /// <returns>The escaped string.</returns>
        [NonAction]
        public string ToEscapeXml(string s)
        {
            string escapeString = s;
            if (!string.IsNullOrWhiteSpace(escapeString))
            {
                escapeString = escapeString.Replace("&", "&amp;");
                escapeString = escapeString.Replace("'", "''"); // Avoiding &apos; for compatibility
                escapeString = escapeString.Replace("\"", "&quot;");
                escapeString = escapeString.Replace(">", "&gt;");
                escapeString = escapeString.Replace("<", "&lt;");
                escapeString = escapeString.Replace("[", "%[[");
                escapeString = escapeString.Replace("]", "]]");
            }
            return escapeString;
        }
        #endregion
}