using System.ComponentModel.DataAnnotations;

namespace ActosAdultosAPI.DTO
{
    public class InscripcionCreateDto
    {
        [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar un cargo válido.")]
        public int CargoId { get; set; }
    }
}
