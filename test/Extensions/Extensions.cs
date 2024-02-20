using Microsoft.AspNetCore.Http.HttpResults;
using TopHackerNewsStories.Helpers;

namespace TopHackerNewsStories.Extensions
{
    public static class Extensions
    {
        static public async Task<Results<Ok<T>, BadRequest,ProblemHttpResult>> ToWebResult<T>(Task<IStatus<T>> operation)
        {
            var g = await operation;
            return g.Ok ? TypedResults.Ok(g.Item) : TypedResults.Problem(g.Message);
        }
    }
}
