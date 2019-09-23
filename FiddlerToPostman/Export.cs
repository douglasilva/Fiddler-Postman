using Fiddler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PostmanSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FiddlerToPostman
{
  [ProfferFormat("Postman", "Exportação para Postman")]
  public class Export : ISessionExporter, IDisposable
  {
    public bool ExportSessions(string sFormat, Session[] oSessions, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
    {
      bool bResult = false;
      string sFilename = null;
      if (dictOptions != null && dictOptions.ContainsKey("Filename"))
      {
        sFilename = (dictOptions["Filename"] as string);
      }
      if (string.IsNullOrEmpty(sFilename))
      {
        sFilename = Utilities.ObtainSaveFilename("Exportar como " + sFormat, "Postman collection (*.postman_collection.json)|*.postman_collection.json");
      }
      if (!string.IsNullOrEmpty(sFilename))
      {
        try
        {
          Postman postman = new Postman();
          postman.Info = new Info();
          postman.Info.Id = Guid.NewGuid();
          postman.Info.Name = "Session Fiddler Exporter";

          postman.Auth = new Auth();
          postman.Auth.Type = "basic";
          postman.Auth.Basic = new List<BasicAuth>() {
            new BasicAuth { Key = "password", Value = "totvs", Type = "string" },
            new BasicAuth { Key = "username", Value = "mestre", Type = "string" }};

          postman.Item = new List<Item>();
          
          foreach (Session oS in oSessions)
          {
            if (null != oS.ViewItem)
            {
              Item request = new Item();
              request.Name = oS.fullUrl;

              request.Event = new List<Event>();

              string responseOutput = string.Empty;
              if (oS.ResponseBody != null)
              {
                oS.utilDecodeResponse(true);

                responseOutput = System.Text.Encoding.UTF8.GetString(oS.ResponseBody);

                if (!string.IsNullOrEmpty(responseOutput))
                {
                  responseOutput = responseOutput.Replace("\\n", "\\\\n");

                  Event test = new Event();
                  test.Listen = "test";
                  test.Script = new Script();
                  test.Script.Id = Guid.NewGuid();
                  test.Script.Exec = new List<string>();

                  test.Script.Exec.Add(@"pm.test(""Resposta igual ao modelo"", function () 
                    { 
                      pm.expect(pm.response.text()).to.include(`" + responseOutput + @"`);
                    });");

                  request.Event.Add(test);
                }
              }


              request.Request = new Request();
              request.Request.Header = new List<KeyValuePair<string, string>>();

              foreach (var requestItem in oS.RequestHeaders)
              {
                request.Request.Header.Add(new KeyValuePair<string, string>(requestItem.Name, requestItem.Value));
              }

              request.Request.Method = oS.RequestMethod;
              request.Request.URL = new URL(oS.fullUrl);

              postman.Item.Add(request);
            }

          }
                    
          using (StreamWriter file = File.CreateText(sFilename))
          {
            JsonSerializer serializer = new JsonSerializer();
            serializer.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
            serializer.ContractResolver = new LowercaseContractResolver();

            serializer.Serialize(file, postman);
          }

          return true;
        }
        catch (Exception ex)
        {
          FiddlerApplication.ReportException(ex, "Falha ao salvar a exportação");
          return false;
        }
      }
      return bResult;
    }

    public void Dispose()
    {
    }
  }
}
