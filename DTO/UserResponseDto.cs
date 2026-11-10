namespace ActosAdultosAPI.DTO
{
    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }

        public bool EmailConfirmed { get; set; }

        public string? Role { get; set; }

        public int? InstitucionId { get; set; }
        public string? InstitucionNombre { get; set; }
    }
}
