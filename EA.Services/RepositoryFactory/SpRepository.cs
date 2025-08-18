using EA.Common;
using EA.Model.CommonModel;
using EA.Model.SpDbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Constants = EA.Common.Constants;

namespace EA.Services.RepositoryFactory;

public static  class SpContext
{
     public static SearchPage<T> BindSearchList<T>(Dictionary<string, object> parameters, List<T> records)
        {
            SearchPage<T> result = new SearchPage<T>
            {
                List = records
            };


            int from = 0, size = 10;
            if (parameters != null)
            {
                if (parameters.ContainsKey(Constants.SearchParameters.PageSize) && parameters.ContainsKey(Constants.SearchParameters.PageStart))
                {
                    size = Convert.ToInt32(parameters[Constants.SearchParameters.PageSize]);
                    from = (Convert.ToInt32(parameters[Constants.SearchParameters.PageStart]) - 1) * size;
                    result.Meta.PageSize = size;
                    result.Meta.Page = Convert.ToInt32(parameters[Constants.SearchParameters.PageStart]);
                }
                else
                {
                    result.Meta.PageSize = size;
                    result.Meta.Page = from + 1;
                }
            }
            else
            {
                result.Meta.PageSize = size;
                result.Meta.Page = from + 1;
            }

            return result;
        }


        public static async Task ExecuteStoreProcedureQuery(this LibraryManagementSpContext catalogDbContext, string sqlQuery, object[] param)
        {
            var saveResult = await catalogDbContext.Set<ExecutreStoreProcedureResult>().FromSqlRaw(sqlQuery, param).ToListAsync();

            if (saveResult != null && saveResult.Count > 0)
            {
                var errorResult = saveResult.FirstOrDefault();
                if (!string.IsNullOrEmpty(errorResult.ErrorMessage))
                {
                    throw new HttpStatusCodeException(StatusCodes.Status500InternalServerError, errorResult.ErrorMessage);
                }
            }
        }
        public static async Task<string> ExecuteStoreProcedureQueryWithSID(this LibraryManagementSpContext catalogDbContext, string sqlQuery, object[] param)
        {
            var saveResult = await catalogDbContext.Set<ExecutreStoreProcedureResultWithSID>().FromSqlRaw(sqlQuery, param).ToListAsync();

            if (saveResult != null && saveResult.Count > 0)
            {
                var result = saveResult.FirstOrDefault();
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    throw new HttpStatusCodeException(StatusCodes.Status500InternalServerError, result.ErrorMessage);
                }
                return result.SID;
            }
            return null;
        }

        public static async Task<string> ExecuteStoreProcedure(this LibraryManagementSpContext catalogDbContext, string sqlQuery, object[] param)
        {
            var response = await catalogDbContext.Set<ExecutreStoreProcedureResult>().FromSqlRaw(sqlQuery, param).ToListAsync(); ;

            if (response == null || response.Count <= 0) return string.Empty;

            var result = response.FirstOrDefault();

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                throw new HttpStatusCodeException(StatusCodes.Status500InternalServerError, result.ErrorMessage);
            }
            return string.IsNullOrEmpty(result.Result) ? string.Empty : result.Result;

        }

        public static async Task<string> ExecuteStoreProcedureWithEntitySID(this LibraryManagementSpContext catalogDbContext, string sqlQuery, object[] param)
        {
            var response = await catalogDbContext.Set<ExecutreStoreProcedureResultWithEntitySID>().FromSqlRaw(sqlQuery, param).ToListAsync();

            if (response == null || response.Count <= 0) return string.Empty;

            var result = response.FirstOrDefault();

            if (result == null) return string.Empty;

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                throw new HttpStatusCodeException(StatusCodes.Status500InternalServerError, result.ErrorMessage);
            }
            return string.IsNullOrEmpty(result.Result) ? string.Empty : result.Result;

        }

        public static async Task<Page> ExecutreStoreProcedureResult(this LibraryManagementSpContext catalogDbContext, string sqlQuery, object[] param)
        {
            Page page = new Page();
            var response = await catalogDbContext.Set<ExecutreStoreProcedureResultList>().FromSqlRaw(sqlQuery, param).ToListAsync();
            if (response != null && response.Count > 0)
            {
                var result = response.FirstOrDefault();

                if (result == null) return page;

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    throw new HttpStatusCodeException(StatusCodes.Status500InternalServerError, result.ErrorMessage);
                }

                page.Meta.TotalResults = result.TotalCount;
                if (!string.IsNullOrWhiteSpace(result.Result))
                {
                    var list = (JArray)JsonConvert.DeserializeObject(result.Result);
                    page.Result = list;
                    return page;
                }
                else
                {
                    page.Result = null;
                }
            }
            return page;
        }
        public static async Task<Page> ExecutreStoreProcedureResultList(this LibraryManagementSpContext catalogDbContext, string sqlQuery, object[] param)
        {
            Page page = new Page();
            var response = await catalogDbContext.Set<ExecutreStoreProcedureResultList>().FromSqlRaw(sqlQuery, param).ToListAsync();
            if (response != null && response.Count > 0)
            {
                var result = response.FirstOrDefault();

                if (result == null) return page;

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    throw new HttpStatusCodeException(StatusCodes.Status500InternalServerError, result.ErrorMessage);
                }

                page.Meta.TotalResults = result.TotalCount;
                if (!string.IsNullOrWhiteSpace(result.Result))
                {
                    var list = JsonConvert.DeserializeObject(result.Result);
                    page.Result = list;
                    return page;
                }
            }
            return page;
 
}
}