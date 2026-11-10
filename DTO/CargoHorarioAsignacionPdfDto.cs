namespace ActosAdultosAPI.DTO
{
    public class CargoHorarioAsignacionPdfDto
    {
        public DayOfWeek DiaSemana { get; set; }

        public TimeOnly HoraInicio { get; set; }

        public TimeOnly HoraFin { get; set; }
    }
}
