namespace presentation.exceptions.http.User;

/// <summary>
/// Internal server exception for User endpoint
/// </summary>
public class UserInternalServerException : BaseHttpException
{
    public UserInternalServerException(Exception innerException)
        : base("An internal error occurred while processing User request", innerException)
    {
    }
}
