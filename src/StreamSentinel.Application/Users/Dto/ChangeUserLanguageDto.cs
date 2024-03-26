using System.ComponentModel.DataAnnotations;

namespace StreamSentinel.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}