namespace WatsonWebserver.Native
{
    using System;
    using System.Net;
    using WatsonWebserver.Core;

    /// <summary>
    /// HTTP context including both request and response.
    /// </summary>
    public class HttpContext : HttpContextBase
    {
        #region Public-Members

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public HttpContext()
        {

        }

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="ctx">HTTP listener context.</param>
        /// <param name="settings">Settings.</param>
        /// <param name="events">Events.</param>
        /// <param name="serializer">Serializer.</param>
        internal HttpContext(
            HttpListenerContext ctx, 
            WebserverSettings settings, 
            WebserverEvents events,
            ISerializationHelper serializer,
            bool KeepAliveResponseData)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            Request = new HttpRequest(ctx, serializer); 
            Response = new HttpResponse(Request, ctx, settings, events, serializer, KeepAliveResponseData); 
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
