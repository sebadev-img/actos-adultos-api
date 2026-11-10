using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class ActoQueryRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool? PermiteCargaCargos { get; set; }
        public bool? PuedeCargarCargos { get; set; }
    }
}
