using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Text.Json;

[ApiController]
[Route("api/v2/[controller]")]
public class PokemonController : ControllerBase
{
    private readonly RestClient _restClient;

    public PokemonController(RestClient restClient)
    {
        _restClient = restClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetPokemonList([FromQuery] int skip = 0,
        [FromQuery] int take = 15)
    { 
        var request = new RestRequest($"?offset={skip}&limit={take}", Method.Get);
        var response = await _restClient.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            using var jsonDoc = JsonDocument.Parse(response.Content);
            var pokemonData = jsonDoc.RootElement;

            var pokemonDataResult = pokemonData.GetProperty("results").EnumerateArray();

            var pokemonList = new List<dynamic>();

            foreach (var pokemon in pokemonDataResult)
            {
                var name = pokemon.GetProperty("name").GetString();
                var url = pokemon.GetProperty("url").GetString();

                pokemonList.Add(new  { 
                    Name = name, 
                    Url = url 
                });
            }

            foreach (var pokemon in pokemonList)
            {
                Console.WriteLine($"Pokemon: {pokemon.Name}");
                Console.WriteLine($"Url: {pokemon.Url}");
            }

            return Ok(new
            {
                Count = pokemonData.GetProperty("count").GetInt32(),
                Results = pokemonList
            });
        }
        else
        {
            return StatusCode((int)response.StatusCode, response.StatusDescription);
        }
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetPokemon(string name)
    {
        var request = new RestRequest(name, Method.Get);
        var response = await _restClient.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            var pokemonData = JsonSerializer.Deserialize<dynamic>(response.Content);

            return Ok(response.Content);
        }
        else
        {
            return StatusCode((int)response.StatusCode, response.StatusDescription);
        }
    }
}
