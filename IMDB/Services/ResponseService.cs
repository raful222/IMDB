using IMDB.Core.Entities;
using IMDB.Models;
using IMDB.ViewModels;

namespace IMDB.Services
{
    public class ResponseService
    {
        public Response MakeResponse(int statusCode, string errorMessage, string additionalInfo, bool isSuccess = false)
        {
            var error = new Errors { Message = errorMessage, Code = statusCode.ToString(), AdditionalInfo = additionalInfo };

            return new Response()
            {
                TraceId = Guid.NewGuid().ToString(),
                IsSuccess = isSuccess,
                StatusCode = statusCode,
                Errors = new List<Errors> { error }
            };
        }
        public ActorResponse MakeActorResponse(Actor actor)
        {
            return new ActorResponse()
            {
                Actor = actor,
                IsSuccess = true,
                StatusCode = 200,
                TraceId = Guid.NewGuid().ToString(),
            };
        }
        public ActorsResponse MakeActorsResponse(List<ActorShortViewModel> actorList)
        {
            return new ActorsResponse()
            {
                Actors = actorList,
                IsSuccess = true,
                StatusCode = 200,
                TraceId = Guid.NewGuid().ToString(),
            };
        }
    }
}
