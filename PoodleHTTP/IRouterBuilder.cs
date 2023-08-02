namespace PSMultiServer.PoodleHTTP
{
    public interface IRouterBuilder
    {
        IRouterBuilder Get(string url, Func<Context, Task> handler);

        IRouterBuilder Post(string url, Func<Context, Task> handler);

        IRouterBuilder Delete(string url, Func<Context, Task> handler);

        IRouterBuilder Put(string url, Func<Context, Task> handler);

        Middleware<Context> Build();
    }
}
