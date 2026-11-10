namespace ActosAdultosAPI.Models
{
    public class CargoHorario: Entity
    {
        public int CargoId { get; set; }
        public Cargo Cargo { get; set; } = null!;

        public DayOfWeek DiaSemana { get; set; }

        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
    }
}
