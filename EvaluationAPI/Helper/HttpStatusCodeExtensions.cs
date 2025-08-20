using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace EvaluationAPI.Helper;

public static class HttpStatusCodeExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <param name="nameValues"></param>
    /// <returns></returns>
    public static string AddOrReplaceQueryParameter(this HttpContext c, params string[] nameValues)
    {
        if (nameValues.Length % 2 != 0)
        {
            throw new Exception("nameValues: has more parameters then values or more values then parameters");
        }
        var qps = new Dictionary<string, StringValues>();
        for (int i = 0; i < nameValues.Length; i += 2)
        {
            qps.Add(nameValues[i], nameValues[i + 1]);
        }
        return c.AddOrReplaceQueryParameters(qps);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <param name="pvs"></param>
    /// <returns></returns>
    public static string AddOrReplaceQueryParameters(this HttpContext c, Dictionary<string, StringValues> pvs)
    {
        var request = c.Request;
        UriBuilder uriBuilder = new UriBuilder
        {
            Scheme = request.Scheme,
            Host = request.Host.Host,
            //Port = request.Host.Port ?? 0,
            Path = request.Path.ToString(),
            Query = request.QueryString.ToString()
        };

        var queryParams = QueryHelpers.ParseQuery(uriBuilder.Query);

        foreach (var (p, v) in pvs)
        {
            queryParams.Remove(p);
            queryParams.Add(p, v);
        }

        uriBuilder.Query = "";
        var allQPs = queryParams.ToDictionary(k => k.Key, k => k.Value.ToString());
        var url = QueryHelpers.AddQueryString(uriBuilder.ToString(), allQPs);

        return url;
    }
}