using Microsoft.AspNetCore.Identity;

namespace ActosAdultosAPI.Models
{
    public class AppUser: IdentityUser
    {
        public Institucion? Institucion { get; set; }
        public Postulante? Postulante { get; set; }
    }
}
