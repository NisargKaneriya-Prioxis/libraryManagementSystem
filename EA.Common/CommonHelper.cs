using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EA.Common;

public static class CommonHelper
{
    public static bool IsValidJson(this string stringValue)
    {
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return false;
        }
        var value = stringValue.Trim();
        if ((value.StartsWith("{") && value.EndsWith("}")) || //For object
            (value.StartsWith("[") && value.EndsWith("]"))) //For array
        {
            try
            {
                var obj = JToken.Parse(value);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        return false;
    }

    public static string DictionaryToXml(Dictionary<string, object> dic, string rootElement = "Root")
    {
        string strXMLResult = string.Empty;

        if (dic != null && dic.Count > 0)
        {
            foreach (KeyValuePair<string, object> pair in dic)
            {
                string safeValue = System.Security.SecurityElement.Escape(pair.Value?.ToString() ?? "");
                strXMLResult += "<" + pair.Key + ">" + safeValue + "</" + pair.Key + ">";
            }

            strXMLResult = "<" + rootElement + ">" + strXMLResult + "</" + rootElement + ">";
        }

        return strXMLResult;
    }
}