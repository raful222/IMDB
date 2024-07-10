using IMDB.Core.Entities;
using IMDB.ViewModels;

namespace IMDB.Models
{
    public class ActorsResponse
    {
        public List<Errors> Errors { get; set; }
        public int StatusCode { get; set; }
        public string TraceId { get; set; }
        public bool IsSuccess { get; set; }
        public List<ActorShortViewModel> Actors { get; set; }
    }
}
