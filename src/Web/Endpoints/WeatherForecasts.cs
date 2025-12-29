using CookiesAuthen.Application.Common.Security;
using CookiesAuthen.Application.WeatherForecasts.Queries.GetWeatherForecasts;
using Microsoft.AspNetCore.Http.HttpResults;
using CookiesAuthen.Web.Extensions;

namespace CookiesAuthen.Web.Endpoints;

public class WeatherForecasts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this).RequireAuthorization();

        group.MapGet("get",GetWeatherForecasts)
             .RequirePermission(ResourceType.WeatherForecast, PermissionAction.View);

        group.MapPost("/create",CreateForecastsV2)
             .RequirePermission(ResourceType.WeatherForecast, PermissionAction.Create);

        group.MapDelete("/delete/{id}", DeleteForecastsV3) // Thêm {id} cho đúng chuẩn REST
             .RequirePermission(ResourceType.WeatherForecast, PermissionAction.Delete);
    }

    public async Task<Ok<IEnumerable<WeatherForecast>>> GetWeatherForecasts(ISender sender)
    {
        var forecasts = await sender.Send(new GetWeatherForecastsQuery());
        return TypedResults.Ok(forecasts);
    }

    public async Task<Ok<IEnumerable<WeatherForecast>>> CreateForecastsV2(ISender sender)
    {
        // Thực tế đoạn này nên là Command tạo mới
        var forecasts = await sender.Send(new GetWeatherForecastsQuery());
        return TypedResults.Ok(forecasts);
    }

    public async Task<Ok<IEnumerable<WeatherForecast>>> DeleteForecastsV3(int id, ISender sender)
    {
        // Thực tế đoạn này nên là Command xóa
        var forecasts = await sender.Send(new GetWeatherForecastsQuery());
        return TypedResults.Ok(forecasts);
    }
}
