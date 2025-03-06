namespace Benchy;

/// <summary>
/// Custom exception class for benchmark comparison errors
/// </summary>
public class BenchmarkComparisonException : Exception
{
    /// <summary>
    /// Creates a new instance of BenchmarkComparisonException
    /// </summary>
    /// <param name="message">The error message</param>
    public BenchmarkComparisonException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance of BenchmarkComparisonException with an inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public BenchmarkComparisonException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
