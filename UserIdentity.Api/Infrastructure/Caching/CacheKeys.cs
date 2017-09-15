namespace UserIdentity.WebUI.Infrastructure.Caching
{
    /// <summary>
    /// Cache key
    /// </summary>
    public static class CacheKeys
    {
        private static string totalNumberOfUsers = "total_number_of_users";

        /// <summary>
        /// Property Total numer of Users
        /// </summary>
        public static string TotalNumberOfUsers {
            get {
                return totalNumberOfUsers;
            }
            set {

                totalNumberOfUsers = value;
            }
        }
    }
}