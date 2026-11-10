using ActosAdultosAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class CargoQueryRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        public int? ActoId { get; set; }

        public int? InstitucionId { get; set; }

        public int? OrientacionId { get; set; }

        public Turno? Turno { get; set; }
    }
}
