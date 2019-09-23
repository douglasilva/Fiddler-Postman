using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace PostmanSDK
{
  public class LowercaseContractResolver : DefaultContractResolver
  {
    protected override string ResolvePropertyName(string propertyName)
    {
      return propertyName.ToLower();
    }
  }

  public class Postman
  {
    public Postman()
    {

    }

    public Info Info { get; set; }

    public List<Item> Item { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Auth Auth { get; set; }

    public string ToJson()
    {
      var serializerSettings = new JsonSerializerSettings();
      serializerSettings.ContractResolver = new LowercaseContractResolver();

      return JsonConvert.SerializeObject(this, Formatting.Indented, serializerSettings);
    }
  }

  public class Auth
  {
    public string Type { get; set; }

    public List<BasicAuth> Basic { get; set; }
  }

  public class BasicAuth
  {
    public string Key { get; set; }

    public string Value { get; set; }

    public string Type { get; set; }
  }

  public class Item
  {
    public string Name { get; set; }

    public List<Event> Event { get; set; }

    public Request Request { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Auth Auth { get; set; }
  }

  public class Event
  {
    public string Listen { get; set; }

    public Script Script { get; set; }

  }

  public class Script
  {
    public Guid Id { get; set; }

    public List<string> Exec { get; set; }

    public string Type { get; set; } = "text/javascript";
  }

  public class Request
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Method { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<KeyValuePair<string, string>> Header { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public URL URL { get; set; }
  }

  public class URL
  {
    public URL(string url)
    {
      Raw = new Uri(url);
    }

    public Uri Raw { get; set; }

    public string Protocol
    {
      get
      {
        return Raw?.Scheme;
      }
    }

    public string Port
    {
      get
      {
        return Convert.ToString(Raw?.Port);
      }
    }

    public string[] Host
    {
      get
      {
        return Raw?.Host.Split('.');
      }
    }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<KeyValuePair<string, string>> Query
    {
      get
      {

        List<KeyValuePair<string, string>> retorno = new List<KeyValuePair<string, string>>();

        if ((Raw != null) && (!string.IsNullOrEmpty(Raw.Query)))
        {
          NameValueCollection query = HttpUtility.ParseQueryString(Raw.Query);
          foreach (var item in query.AllKeys)
          {
            retorno.Add(new KeyValuePair<string, string>(item, Convert.ToString(query[item])));
          }
        }

        return retorno;
      }
    }

    public IEnumerable<string> Path
    {
      get
      {
        return Raw?.LocalPath.Split('/').Where(x => !string.IsNullOrEmpty(x));
      }
    }
  }


  public class Info
  {
    [JsonProperty("_postman_id")]
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Schema { get; set; } = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json";
  }
}
