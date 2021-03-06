﻿using System;
using System.Net.Http.Headers;
using System.Web.Http;

namespace LibraryCore
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            Console.WriteLine("** registerins app **");
            //config.Formatters.Clear();
            //config.Formatters.Add(new XmlMediaTypeFormatter());
            //config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            //var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //formatter.SerializerSettings.ContractResolver =
            //    new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.UseXmlSerializer = true;
        }
    }
}