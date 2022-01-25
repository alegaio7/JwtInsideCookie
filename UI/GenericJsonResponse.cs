namespace UI
{
    public class GenericJsonResponse
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; } = (int)System.Net.HttpStatusCode.OK;
        public dynamic Contents { get; set; }
    }
}