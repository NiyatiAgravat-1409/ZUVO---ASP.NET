using Microsoft.AspNetCore.Identity;

namespace ZUVO_MVC_.Models
{
    public class Users: IdentityUser 
    {
        public string FullName { get; set; }
    }
}
