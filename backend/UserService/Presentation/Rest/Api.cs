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

            api.MapPut("current/locale", ChangeCurrentUserLocaleAsync);
            api.MapGet(string.Empty, GetUsersPagedAsync);
            api.MapGet("{userId}", GetUserAsync);
            api.MapGet("bulk/{userIds}", GetUsersBulkAsync);

            return app;
        }

        public IEndpointRouteBuilder MapApi()
        {
            app.UserApi();

            return app;
        }
    }
}
