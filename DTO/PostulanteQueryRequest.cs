using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class PostulanteQueryRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        [MaxLength(150)]
        public string? Search { get; set; }

        public int? CiudadId { get; set; }
    }
}
