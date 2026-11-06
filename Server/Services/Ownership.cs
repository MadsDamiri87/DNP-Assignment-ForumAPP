using ServiceContracts;

namespace Services;

internal static class Ownership
{
    internal static void Ensure(int ownerId, Caller caller, string what)
    {
        if (caller.IsAdmin || ownerId == caller.UserId)
        {
            return;
        }

        throw new UnauthorizedAccessException(
            $"You do not have authorization to change this {what}.");
    }
}