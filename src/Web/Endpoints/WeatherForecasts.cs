using CookiesAuthen.Application.WeatherForecasts.Queries.GetWeatherForecasts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CookiesAuthen.Web.Endpoints;

public class WeatherForecasts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetWeatherForecasts)
            .MapGet(CreateForecastsV2)
            .MapGet(DeleteForecastsV3);

    }

    public async Task<Ok<IEnumerable<WeatherForecast>>> GetWeatherForecasts(ISender sender)
    {
        var forecasts = await sender.Send(new GetWeatherForecastsQuery());
        
        return TypedResults.Ok(forecasts);
    }
    public async Task<Ok<IEnumerable<WeatherForecast>>> CreateForecastsV2(ISender sender)
    {
        var forecasts = await sender.Send(new GetWeatherForecastsQuery());

        return TypedResults.Ok(forecasts);
    }
    public async Task<Ok<IEnumerable<WeatherForecast>>> DeleteForecastsV3(ISender sender)
    {
        var forecasts = await sender.Send(new GetWeatherForecastsQuery());

        return TypedResults.Ok(forecasts);
    }
}
