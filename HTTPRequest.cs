using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace DNWS
{
  public class HTTPRequest
  {
    protected String _url;
    protected String _filename;
    protected static ConcurrentDictionary<String, String> _propertyListDictionary = null;
    protected static Dictionary<String, String> _requestListDictionary = null;

    protected String _body;

    protected int _status;

    protected String _method;

    public String Url
    {
      get { return _url;}
    }

    public String Filename
    {
      get { return _filename;}
    }

    public String Body
    {
      get {return _body;}
    }

    public int Status
    {
      get {return _status;}
    }

    public String Method
    {
      get {return _method;}
    }
    public HTTPRequest(String request)
    {
      _propertyListDictionary = new ConcurrentDictionary<String, String>();
      String[] lines = Regex.Split(request, "\\n");

      if(lines.Length == 0) {
        _status = 500;
        return;
      }

      String[] statusLine = Regex.Split(lines[0], "\\s");
      if(statusLine.Length != 4) { // too short something is wrong
        _status = 401;
        return;
      }
      if (!statusLine[0].ToLower().Equals("get"))
      {
        _method = "GET";
      } else if(!statusLine[0].ToLower().Equals("post")) {
        _method = "POST";
      } else {
        _status = 501;
        return;
      }
      _status = 200;

      _url = statusLine[1];
      String[] urls = Regex.Split(_url, "/");
      _filename = urls[urls.Length - 1];
      String[] parts = Regex.Split(_filename, "[?]");
      if (parts.Length > 1 && parts[1].Contains('&'))
      {
        //Ref: http://stackoverflow.com/a/4982122
        _requestListDictionary = parts[1].Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0].ToLower(), x => x[1]);
      } else{
        _requestListDictionary = new Dictionary<String, String>();
      }

      if(lines.Length == 1) return;

      for(int i = 1; i != lines.Length; i++) {
        String[] pair = Regex.Split(lines[i], ": "); //FIXME
        if(pair.Length == 0) continue;
        if(pair.Length == 1) { // handle post body
          if(pair[0].Length > 1) { //FIXME, this is a quick hack
            Dictionary<String, String> _bodys = pair[0].Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0].ToLower(), x => x[1]);
            _requestListDictionary = _requestListDictionary.Concat(_bodys).ToDictionary(x=>x.Key, x=>x.Value);
          }
        } else { // Length == 2, GET url request
          addProperty(pair[0], pair[1]);
        }
      }
    }
    public String getPropertyByKey(String key)
    {
      if(_propertyListDictionary.ContainsKey(key.ToLower())) {
        return _propertyListDictionary[key.ToLower()];
      } else {
        return null;
      }
    }
    public String getRequestByKey(String key)
    {
      if(_requestListDictionary.ContainsKey(key.ToLower())) {
        return _requestListDictionary[key.ToLower()];
      } else {
        return null;
      }
    }

    public void addProperty(String key, String value)
    {
      _propertyListDictionary[key.ToLower()] = value;
    }
    public void addRequest(String key, String value)
    {
      _requestListDictionary[key.ToLower()] = value;
    }
  }
}