using System;
using System.Collections.Generic;
using System.Text;

namespace MatchFinder.Infrastructure.Common;

public class ServiceResult
{
    public bool Succeeded { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static ServiceResult Success() => new() { Succeeded = true };
    public static ServiceResult Failure(string error) => new() { Succeeded = false, ErrorMessage = error };
}