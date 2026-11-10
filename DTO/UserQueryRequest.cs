using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class UserQueryRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        public string? Role { get; set; }

        public bool SoloDisponiblesInstitucion { get; set; } = false;
    }
}
