using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PSMultiServer.PoodleHTTP
{
    public class RouterBuilder : IRouterBuilder
    {
        private static readonly Regex PathParameterRegex = new("(?<=/):(.+?)(?:(?=/)|$)", RegexOptions.Compiled);
        private static readonly char[] Separator = { '/' };

        private readonly List<(string url, Func<Context, Task> handler)> _getRoutes = new();
        private readonly List<(string url, Func<Context, Task> handler)> _postRoutes = new();
        private readonly List<(string url, Func<Context, Task> handler)> _deleteRoutes = new();
        private readonly List<(string url, Func<Context, Task> handler)> _putRoutes = new();

        public IRouterBuilder Get(string url, Func<Context, Task> handler)
        {
            _getRoutes.Add((url, handler));
            return this;
        }

        public IRouterBuilder Post(string url, Func<Context, Task> handler)
        {
            _postRoutes.Add((url, handler));
            return this;
        }

        public IRouterBuilder Delete(string url, Func<Context, Task> handler)
        {
            _deleteRoutes.Add((url, handler));
            return this;
        }

        public IRouterBuilder Put(string url, Func<Context, Task> handler)
        {
            _putRoutes.Add((url, handler));
            return this;
        }

        public Middleware<Context> Build()
        {
            var getRoutes = GenerateRegexRoutes(_getRoutes);
            var postRoutes = GenerateRegexRoutes(_postRoutes);
            var deleteRoutes = GenerateRegexRoutes(_deleteRoutes);
            var putRoutes = GenerateRegexRoutes(_putRoutes);

            return async (ctx, next) =>
            {
                bool handled = ctx.Request.HttpMethod.ToUpperInvariant() switch
                {
                    HttpMethods.Get => await TryMatch(getRoutes, ctx),
                    HttpMethods.Post => await TryMatch(postRoutes, ctx),
                    HttpMethods.Delete => await TryMatch(deleteRoutes, ctx),
                    HttpMethods.Put => await TryMatch(putRoutes, ctx),
                    _ => false,
                };

                if (!handled)
                {
                    await next();
                }
            };
        }

        private static IReadOnlyList<(Regex regex, Func<Context, Task> handler)> GenerateRegexRoutes(IEnumerable<(string url, Func<Context, Task> handler)> routes)
        {
            var toSortRoutes = routes
                .Select(item => (
                    fragmentCount: item.url.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Length,
                    item.url,
                    item.handler))
                .ToList();
            toSortRoutes.Sort((x, y) => y.fragmentCount - x.fragmentCount);

            return toSortRoutes
                .Select(item => (regex: GetPathRegex(item.url), item.handler))
                .ToList();
        }

        private static Regex GetPathRegex(string url)
        {
            HashSet<string> parameterNames = new();
            string urlRegex = PathParameterRegex
                .Replace(url.Trim('/'), match =>
                {
                    if (!parameterNames.Add(match.Value))
                    {
                        throw new ArgumentException($"Cannot contains duplicate variable name: '{match.Value}'.", nameof(url));
                    }

                    return $"(?<{match.Groups[1]}>.+?)";
                });

            return new Regex($"{urlRegex}$", RegexOptions.Compiled);
        }

        private static async Task<bool> TryMatch(IEnumerable<(Regex regex, Func<Context, Task> handler)> routes, Context ctx)
        {
            string requestPath = ctx.Request.Url.LocalPath.ToLowerInvariant();

            foreach ((Regex regex, Func<Context, Task> handler) in routes)
            {
                Match match = regex.Match(requestPath);
                if (match.Success)
                {
                    NameValueCollection query = HttpUtility.ParseQueryString(ctx.Request.Url.Query, Encoding.UTF8);

                    ctx.TryGetParameter = (string key, out string value) =>
                    {
                        Group group = match.Groups[key];
                        value = HttpUtility.UrlDecode(group.Value);

                        if (!group.Success)
                        {
                            value = query.Get(key);
                        }

                        return !string.IsNullOrEmpty(value);
                    };

                    await handler(ctx);
                    return true;
                }
            }

            return false;
        }
    }
}
