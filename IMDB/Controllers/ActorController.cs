using AutoMapper;
using IMDB.Core.Data;
using IMDB.Core.Entities;
using IMDB.Models;
using IMDB.Services;
using IMDB.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.Interactions;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace IMDB.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ActorController : ControllerBase
    {
        private readonly ILogger<ActorController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMDbArtorScrapService _artorScrapService;
        private readonly IMapper _mapper;
        private readonly ResponseService _responseService;
        private readonly IHttpClientFactory _httpClientFactory;

        public ActorController(ILogger<ActorController> logger
           , ApplicationDbContext context,
           IMDbArtorScrapService artorScrapService,
        IMapper mapper,
            ResponseService responseService,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _context = context;
            _artorScrapService = artorScrapService;
            _mapper = mapper;
            _responseService = responseService;
            _httpClientFactory = httpClientFactory;
        }




        [HttpGet("GetActorScrap")]
        public async Task<IActionResult> GetActorScrap()
        {
            try
            { 
                var url = "https://www.imdb.com/list/ls054840033/";
            var actorList = await _artorScrapService.ScrapeArtorData(url);

            if (actorList != null && actorList.Any())
            {
                await _context.AddRangeAsync(actorList);
                await _context.SaveChangesAsync();
            }

            return Ok("Actors scraped and saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing actors.");
                return StatusCode(500, "An error occurred while processing actors.");
            }
        }


        [HttpGet("GetListOfAllActors")]
        [ProducesResponseType(typeof(ActorsResponse), 201)]
        [ProducesResponseType(typeof(Response), 500)]
        public async Task<IActionResult> GetListOfAllActors(
            [FromQuery] string? name = null,
            [FromQuery] int? minRank = null,
            [FromQuery] int? maxRank = null,
            [FromQuery] string? provider = null,
             [FromHeader(Name = "Skip")] int skip = 0,
    [FromHeader(Name = "PageSize")] int pageSize = 20)
        {
            var response = new Response();
            try
            {
                var query = _context.Actors.AsQueryable();

                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(a => a.Name.Contains(name));
                }
                if (!string.IsNullOrEmpty(provider))
                {
                    query = query.Where(a => a.Source.Contains(provider));
                }

                if (minRank.HasValue)
                {
                    query = query.Where(a => a.Rank >= minRank);
                }

                if (maxRank.HasValue)
                {
                    query = query.Where(a => a.Rank <= maxRank);
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var actors = await query
                    .OrderBy(a => a.Rank)
                    .Skip((skip) * pageSize)
                    .Take(pageSize)
                    .Select(a => new ActorShortViewModel { Id = a.Id, Name = a.Name })
                    .ToListAsync();

                var actoursResponse = _responseService.MakeActorsResponse(actors);
                return Ok(actoursResponse);
            }
            catch (Exception ex)
            {
                response = _responseService.MakeResponse(500, ex.Message, ex.Message);
                return StatusCode(500, response);
            }
        }

        [ProducesResponseType(typeof(ActorResponse), 200)]
        [ProducesResponseType(typeof(Response), 404)]
        [ProducesResponseType(typeof(Response), 500)]
        [HttpGet("GetActorDetails/{id}")]
        public async Task<IActionResult> GetActorDetails(string id)
        {
            var response = new Response();
            try
            {
                var actor = await _context.Actors.FindAsync(id);
                if (actor == null)
                {
                    response = _responseService.MakeResponse(404, "Actor not found", "");
                    return NotFound(response);
                }
                var actorResponse = _responseService.MakeActorResponse(actor);
                return CreatedAtAction(nameof(GetActorDetails), new { id = actorResponse.Actor.Id }, actorResponse);
            }
            catch (Exception ex)
            {
                response = _responseService.MakeResponse(500, ex.Message, ex.Message);
                return StatusCode(500, response);
            }
        }


        [HttpGet("RankExists")]
        [ProducesResponseType(typeof(ActorResponse), 200)]
        [ProducesResponseType(typeof(Response), 404)]
        [ProducesResponseType(typeof(Response), 500)]
        public async Task<IActionResult> GetRankExists()
        {
            var response = new Response();
            try
            {
                var actorRanks = await _context.Actors.OrderBy(x => x.Rank).Select(x => x.Rank).ToListAsync();
                if (actorRanks == null)
                {
                    response = _responseService.MakeResponse(404, "Actor not found", "");
                    return NotFound(response);
                }
                return Ok(actorRanks);
            }
            catch (Exception ex)
            {
                response = _responseService.MakeResponse(500, ex.Message, ex.Message);
                return StatusCode(500, response);
            }
        }

        [ProducesResponseType(typeof(ActorResponse), 200)]
        [ProducesResponseType(typeof(Response), 400)]
        [ProducesResponseType(typeof(Response), 404)]
        [ProducesResponseType(typeof(Response), 422)]
        [ProducesResponseType(typeof(Response), 409)]
        [ProducesResponseType(typeof(Response), 500)]
        [HttpPost("AddActor")]
        public async Task<IActionResult> AddActor([FromBody] ActorViewModel actorModel)
        {
            if (actorModel == null)
            {
                var response = _responseService.MakeResponse(400, "Actor details are null", "");
                return BadRequest(response);
            }
            if (actorModel.Rank == 0 || actorModel.Name == "string")
            {
                var response = _responseService.MakeResponse(422, "Rank cannot be 0 or Name cannot be 'string'", "");
                return UnprocessableEntity(response);
            }
            if (_context.Actors.Any(x => x.Rank == actorModel.Rank))
            {
                var response = _responseService.MakeResponse(409, $"An actor with rank {actorModel.Rank} already exists. Please choose a different rank.", "");

                return Conflict(response);
            }

            var actor = _mapper.Map<Actor>(actorModel);

            try
            {
                await _context.Actors.AddAsync(actor);
                await _context.SaveChangesAsync();

                var response = _responseService.MakeActorResponse(actor);
                return CreatedAtAction(nameof(GetActorDetails), new { id = response.Actor.Id }, response);
            }
            catch (Exception ex)
            {
                var response = _responseService.MakeResponse(500, ex.Message, ex.Message);
                return StatusCode(500, response);
            }
        }

        [ProducesResponseType(typeof(ActorResponse), 200)]
        [ProducesResponseType(typeof(Response), 400)]
        [ProducesResponseType(typeof(Response), 404)]
        [ProducesResponseType(typeof(Response), 409)]
        [ProducesResponseType(typeof(Response), 422)]
        [ProducesResponseType(typeof(Response), 500)]
        [HttpPut("UpdateActor/{id}")]
        public async Task<IActionResult> UpdateActor(string id, [FromBody] ActorViewModel actorModel)
        {
            if (actorModel == null)
            {
                var response = _responseService.MakeResponse(400, "Actor details are null", "");
                return BadRequest(response);
            }

            var existingActor = await _context.Actors.FindAsync(id);
            if (existingActor == null)
            {
                var response = _responseService.MakeResponse(404, "Actor not found", "");
                return NotFound(response);
            }

            if (_context.Actors.Any(x => x.Rank == actorModel.Rank && x.Id != id))
            {
                var response = _responseService.MakeResponse(409, $"An actor with rank {actorModel.Rank} already exists. Please choose a different rank.", "");
                return Conflict(response);
            }
            try
            {
                existingActor.Name = actorModel.Name;
                existingActor.Details = actorModel.Details;
                existingActor.Type = actorModel.Type;
                existingActor.Rank = actorModel.Rank;
                existingActor.Source = actorModel.Source;

                _context.Actors.Update(existingActor);
                await _context.SaveChangesAsync();
                var response = _responseService.MakeActorResponse(existingActor);
                return CreatedAtAction(nameof(GetActorDetails), new { id = response.Actor.Id }, response);
            }
            catch (Exception ex)
            {
                var response = _responseService.MakeResponse(500, ex.Message, ex.Message);
                return StatusCode(500, response);
            }

        }

        #region delete

        [HttpDelete("DeleteActor/{id}")]
        [ProducesResponseType(typeof(ActorResponse), 200)]
        [ProducesResponseType(typeof(Response), 404)]
        [ProducesResponseType(typeof(Response), 500)]
        public async Task<IActionResult> DeleteActor(string id)
        {
            var response = new Response();
            try
            {
                var actor = await _context.Actors.FindAsync(id);
                if (actor == null)
                {
                    response = _responseService.MakeResponse(404, "Actor not found", "");
                    return NotFound(response);
                }

                _context.Actors.Remove(actor);
                await _context.SaveChangesAsync();
                response = _responseService.MakeResponse(200, "", "", true);

                return StatusCode(200, response);
            }
            catch (Exception ex)
            {
                response = _responseService.MakeResponse(500, ex.Message, ex.Message);
                return StatusCode(500, response);
            }
        }

        [HttpDelete("DeleteAll")]
        [ProducesResponseType(typeof(ActorResponse), 200)]
        [ProducesResponseType(typeof(Response), 404)]
        [ProducesResponseType(typeof(Response), 500)]
        public async Task<IActionResult> DeleteAllActor()
        {
            var response = new Response();
            try
            {
                var actor = await _context.Actors.ToListAsync();

                if (actor == null)
                {
                    response = _responseService.MakeResponse(404, "Actor is empty", "");
                    return NotFound(response);
                }

                _context.Actors.RemoveRange(actor);
                await _context.SaveChangesAsync();

                response = _responseService.MakeResponse(200, "", "", true);

                return StatusCode(200, response);
            }
            catch (Exception ex)
            {
                response = _responseService.MakeResponse(500, ex.Message, ex.Message);
                return StatusCode(500, response);
            }
        }

        #endregion

    }
}
