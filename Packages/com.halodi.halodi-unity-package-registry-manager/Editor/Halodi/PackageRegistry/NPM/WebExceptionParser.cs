using System.Net;

namespace Halodi.PackageRegistry.NPM
{
    public class WebExceptionParser
    {
        public static string ParseWebException(WebException e)
        {
            if (e.Status == WebExceptionStatus.ProtocolError)
            {
                HttpWebResponse response = (HttpWebResponse)e.Response;

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return response.StatusCode + ": Invalid credentials.";
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.Conflict:
                        return response.StatusCode + ": Check if version already exists on server.";
                    default:
                        return response.StatusCode + ": Unknown error. Try again.";

                }

            }
            else
            {
                if (e.InnerException != null)
                {
                    return e.InnerException.Message;
                }
                else
                {
                    return e.Message;
                }
            }


        }
    }
}