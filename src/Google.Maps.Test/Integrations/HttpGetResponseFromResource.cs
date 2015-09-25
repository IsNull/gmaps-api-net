using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using Google.Maps.Internal;

namespace Google.Maps.Test.Integrations
{
	public class HttpGetResponseFromResource : HttpGetResponse
	{
		System.Reflection.Assembly S_testAssembly = Assembly.GetAssembly(typeof(HttpGetResponseFromResource));

        public HttpGetResponseFromResource(Uri uri, bool refreshCache)
            : base(uri, refreshCache)
		{
		}

		private string _resourcePath;
		public string BaseResourcePath { get { return this._resourcePath; } set { this._resourcePath = value; } }




	    protected override string AsString()
	    {
            using (var sr = new StreamReader(GetResourceStream()))
	        {
	            return sr.ReadToEnd();
	        }
	    }

	    protected override Task<string> AsStringAsync()
	    {
            using (var sr = new StreamReader(GetResourceStream()))
            {
                return sr.ReadToEndAsync();
            }
	    }

	    private Stream GetResourceStream()
	    {
            var uri = RequestUri;

            string outputType = uri.Segments[uri.Segments.Length - 1];

            var queryString = new StringBuilder(uri.OriginalString.Substring(uri.OriginalString.IndexOf("?") + 1));
            queryString.Replace("&sensor=false", ""); //clear off sensor=false
            queryString.Replace("&sensor=true", ""); // clear off sensor=true

            //have to replace any remaining ampersands with $ due to filename limitations.
            queryString.Replace("&", "$").Replace("|", "!").Replace("%", "~");

            string resourcePath = this.BaseResourcePath +
                                  string.Format(".{0}_queries.{1}.{0}", outputType, queryString.ToString());

            Stream resourceStream = S_testAssembly.GetManifestResourceStream(resourcePath);

            if (resourceStream == null)
            {
                string message = string.Format(
                    @"Failed to find resource for query '{0}'.
                    BaseResourcePath: '{2}'
                    Resource path used: '{1}'
                    Ensure a file exists at that resource path and the file has its Build Action set to ""Embedded Resource"".",
                    queryString.ToString(), resourcePath, BaseResourcePath);
                throw new FileNotFoundException(message);
            }

	        return resourceStream;
	    }
	}

	public class HttpGetResponseFromResourceFactory : Http.HttpGetResponseFactory
	{
		public HttpGetResponseFromResourceFactory(string baseResourcePath)
		{
			this.BaseResourcePath = baseResourcePath;
		}

		public string BaseResourcePath { get; set; }

        public override HttpGetResponse CreateResponse(Uri uri, bool forceNoCache = false)
		{
            return new HttpGetResponseFromResource(uri, forceNoCache) { BaseResourcePath = this.BaseResourcePath };
		}
	}
}
