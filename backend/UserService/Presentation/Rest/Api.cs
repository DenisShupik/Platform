using Shared.Presentation.Extensions;

namespace UserService.Presentation.Rest;

public static partial class Api
{
    extension(IEndpointRouteBuilder app)
    {
        private IEndpointRouteBuilder UserApi()
        {
            var api = app
                .MapGroup("api/users")
                .WithTags(nameof(UserApi))
                .WithAutoNames();

            api.MapPut("current/locale", ChangeCurrentUserLocaleAsync)
                .WithSummary("Change the current user's locale");
            api.MapGet(string.Empty, GetUsersPagedAsync).WithSummary("Get users");
            api.MapGet("{userId}", GetUserAsync).WithSummary("Get a user");
            api.MapGet("bulk/{userIds}", GetUsersBulkAsync).WithSummary("Get users by ID");

            return app;
        }

        public IEndpointRouteBuilder MapApi()
        {
            app.UserApi();

            return app;
        }
    }
}
