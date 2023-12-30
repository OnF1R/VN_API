namespace VN_API.Models.Pagination
{
    public class PaginationParams
    {
        /// <summary>
        /// Maximum items on page
        /// </summary>
        private const int _maxItemPerPage = 50;
        /// <summary>
        /// Current items on page
        /// </summary>
        private int itemsPerPage;
        /// <summary>
        /// Current page
        /// </summary>
        public int Page { get; set; } = 1;
        /// <summary>
        /// Validated value of itemsPerPage
        /// </summary>
        public int ItemsPerPage
        {
            get => itemsPerPage;
            set => itemsPerPage = value > _maxItemPerPage ? _maxItemPerPage : value;
        }
    }
}
