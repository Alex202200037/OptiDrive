namespace OptiDrive.Web.Infrastructure;

public static class SessionExtensions
{
    private const string UserIdKey = "OptiDrive.UserId";
    private const string PendingMfaUserIdKey = "OptiDrive.PendingMfaUserId";

    public static void SetCurrentUserId(this ISession session, Guid userId)
        => session.SetString(UserIdKey, userId.ToString());

    public static Guid? GetCurrentUserId(this ISession session)
    {
        var raw = session.GetString(UserIdKey);
        return Guid.TryParse(raw, out var userId) ? userId : null;
    }

    public static void ClearCurrentUser(this ISession session)
        => session.Remove(UserIdKey);

    public static void SetPendingMfaUserId(this ISession session, Guid userId)
        => session.SetString(PendingMfaUserIdKey, userId.ToString());

    public static Guid? GetPendingMfaUserId(this ISession session)
    {
        var raw = session.GetString(PendingMfaUserIdKey);
        return Guid.TryParse(raw, out var userId) ? userId : null;
    }

    public static void ClearPendingMfaUser(this ISession session)
        => session.Remove(PendingMfaUserIdKey);
}
