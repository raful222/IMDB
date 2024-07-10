namespace IMDB.Models
{
    public class Response
    {
        public List<Errors> Errors { get; set; }
        public int StatusCode { get; set; }
        public string TraceId { get; set; }
        public bool IsSuccess { get; set; } = false;
    }
}
