using DowntimeTracker.Services;

public class UserLoginMiddleware
{
    private readonly RequestDelegate _next;

    public UserLoginMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {

        string userAd = context.User.Identity.Name; // or however you get the user ID
        await userService.UpdateUserLoginAsync(userAd);

        await _next(context);
    }
}