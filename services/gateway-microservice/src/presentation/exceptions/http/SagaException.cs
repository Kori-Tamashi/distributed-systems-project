namespace presentation.exceptions.http;

using presentation.exceptions.http;

/// <summary>
/// Exception thrown when a SAGA operation fails
/// Contains information about compensating transactions
/// </summary>
public class SagaException : BaseHttpException
{
    /// <summary>
    /// List of compensated operations
    /// </summary>
    public List<string> CompensatedOperations { get; }

    /// <summary>
    /// The step where SAGA failed
    /// </summary>
    public int FailedStep { get; }

    /// <summary>
    /// Constructor with details
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="failedStep">Step number where failure occurred</param>
    /// <param name="compensatedOperations">List of compensated operations</param>
    public SagaException(string message, int failedStep, List<string> compensatedOperations)
        : base(500, "SAGA_FAILED", message)
    {
        FailedStep = failedStep;
        CompensatedOperations = compensatedOperations ?? new List<string>();
    }

    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="failedStep">Step number where failure occurred</param>
    /// <param name="compensatedOperations">List of compensated operations</param>
    /// <param name="innerException">Inner exception</param>
    public SagaException(string message, int failedStep, List<string> compensatedOperations, Exception innerException)
        : base(500, "SAGA_FAILED", message, innerException)
    {
        FailedStep = failedStep;
        CompensatedOperations = compensatedOperations ?? new List<string>();
    }
}

/// <summary>
/// Exception thrown when SAGA compensation fails
/// </summary>
public class SagaCompensationException : BaseHttpException
{
    /// <summary>
    /// The operation that failed to compensate
    /// </summary>
    public string FailedOperation { get; }

    /// <summary>
    /// Constructor with details
    /// </summary>
    /// <param name="operation">Operation that failed to compensate</param>
    /// <param name="message">Error message</param>
    public SagaCompensationException(string operation, string message)
        : base(500, "SAGA_COMPENSATION_FAILED", $"Failed to compensate operation: {operation} - {message}")
    {
        FailedOperation = operation;
    }

    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="operation">Operation that failed to compensate</param>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public SagaCompensationException(string operation, string message, Exception innerException)
        : base(500, "SAGA_COMPENSATION_FAILED", $"Failed to compensate operation: {operation} - {message}", innerException)
    {
        FailedOperation = operation;
    }
}
