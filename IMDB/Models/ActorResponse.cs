using IMDB.Core.Entities;
using IMDB.ViewModels;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace IMDB.Models
{
    public class ActorResponse
    {
        public List<Errors> Errors { get; set; }
        public int StatusCode { get; set; }
        public string TraceId { get; set; }
        public bool IsSuccess { get; set; }
        public Actor Actor { get; set; }
    }
}
