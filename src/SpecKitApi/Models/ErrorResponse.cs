namespace SpecKitApi.Models;

public sealed record ErrorResponse(string Message, string Code, string CorrelationId);
