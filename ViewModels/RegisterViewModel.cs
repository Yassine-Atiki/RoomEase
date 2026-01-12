using System.ComponentModel.DataAnnotations;

namespace RoomEase.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
        [StringLength(50, ErrorMessage = "Le nom d'utilisateur doit contenir entre {2} et {1} caractères", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Le nom d'utilisateur ne peut contenir que des lettres, chiffres et underscores")]
        [Display(Name = "Nom d'utilisateur")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "L'adresse e-mail est requise")]
        [EmailAddress(ErrorMessage = "Format d'e-mail invalide")]
        [Display(Name = "Adresse e-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le nom complet est requis")]
        [Display(Name = "Nom complet")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string FullName { get; set; }

        [Display(Name = "Département / Service")]
        [StringLength(100, ErrorMessage = "Le département ne peut pas dépasser 100 caractères")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [StringLength(100, ErrorMessage = "Le {0} doit contenir au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmPassword { get; set; }
    }
}
