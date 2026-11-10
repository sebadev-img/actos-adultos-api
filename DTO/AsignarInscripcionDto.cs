using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class AsignarInscripcionDto
    {
        [Range(1, int.MaxValue)]
        public int InscripcionId { get; set; }
    }
}
