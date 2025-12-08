using System.ComponentModel.DataAnnotations;

namespace RoomEase.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'adresse e-mail est requise")]
        [EmailAddress(ErrorMessage = "Format d'e-mail invalide")]
        [Display(Name = "Adresse e-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [Display(Name = "Se souvenir de moi")]
        public bool RememberMe { get; set; }
    }
}
