using BandCommunity.Shared.Utility;
using Microsoft.AspNetCore.Mvc;

namespace BandCommunity.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly CountryStateService _countryStateService;
    
    public LocationController(CountryStateService countryStateService)
    {
        _countryStateService = countryStateService;
    }
    
    [HttpGet("countries")]
    [ResponseCache(Duration = 86400)] // Cache for 24 hours
    public async Task<IActionResult> GetAllCountries()
    {
        var result = await _countryStateService.GetAllCountries();
        return Content(result, "application/json");
    }
    
    [HttpGet("states/{countryCode}")]
    public async Task<IActionResult> GetStatesByCountry(string countryCode)
    {
        var result = await _countryStateService.GetStatesByCountry(countryCode);
        return Content(result, "application/json");
    }
}