// Copyright (C) 2016 by Barend Erasmus and donated to the public domain
using System;
using WatsonWebserver.Core;

namespace ApacheNet.RouteHandlers
{
    public class Route
    {
        #region Properties

        public string? Name { get; set; } // descriptive name for debugging
        public string? UrlRegex { get; set; }
        public string? Method { get; set; }
        public string? Host { get; set; }
        public Func<HttpContextBase, bool?>? Callable { get; set; }

        #endregion
    }
}
