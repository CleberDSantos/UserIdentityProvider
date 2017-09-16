namespace UserIdentity.WebUI.Infrastructure.Caching
{
    /// <summary>
    /// Cache key
    /// </summary>
    public static class CacheKeys
    {
        private static readonly string totalNumberOfUsers = "total_number_of_users";

        /// <summary>
        /// TotalNumberOfUsers property
        /// </summary>
        public static string TotalNumberOfUsers => totalNumberOfUsers;
    }
}