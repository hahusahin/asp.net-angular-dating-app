namespace API.Helpers
{
    // This is the type definition of pagination headers that we want to send to client
    public class PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPages)
    {
        public int CurrentPage { get; set; } = currentPage;
        public int ItemsPerPage { get; set; } = itemsPerPage;
        public int TotalItems { get; set; } = totalItems;
        public int TotalPages { get; set; } = totalPages;
    }
}
