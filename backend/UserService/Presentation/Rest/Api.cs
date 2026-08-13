using Shared.Presentation.Extensions;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Presentation.Rest.Dtos;

namespace UserService.Presentation.Rest;

public static partial class Api
{
    extension(IEndpointRouteBuilder app)
    {
        private IEndpointRouteBuilder UserApi()
        {
            var api = app
                .MapGroup("api/users")
                .WithTags(nameof(UserApi));

            api.MapPut<ChangeCurrentUserLocaleRequest, ChangeCurrentUserLocaleCommandHandler>("current/locale");
            api.MapGet<GetUsersPagedRequest, GetUsersPagedQueryHandler<UserDto>>(string.Empty);
            api.MapGet<GetUserRequest, GetUserQueryHandler<UserDto>>("{userId}");
            api.MapGet<GetUsersBulkRequest, GetUsersBulkQueryHandler<UserDto>>("bulk/{userIds}");

            return app;
        }

        public IEndpointRouteBuilder MapApi()
        {
            app.UserApi();

            return app;
        }
    }
}
