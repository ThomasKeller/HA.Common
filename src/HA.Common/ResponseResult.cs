using System.Net;

namespace HA.Common;

public class ResponseResult
{
    public bool IsSuccessful { get; set; }
    public HttpStatusCode? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
}