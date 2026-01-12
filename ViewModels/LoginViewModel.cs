using System.ComponentModel.DataAnnotations;

namespace RoomEase.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'identifiant est requis")]
        [Display(Name = "Email ou nom d'utilisateur")]
        public string EmailOrUserName { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [Display(Name = "Se souvenir de moi")]
        public bool RememberMe { get; set; }
    }
}
