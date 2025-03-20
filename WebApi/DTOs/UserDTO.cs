namespace WebAPI.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public bool IsAdmin { get; set; }
        
        // FullName is a computed property that combines FirstName and LastName
        public string FullName 
        { 
            get 
            {
                if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
                    return Username;
                    
                return $"{FirstName} {LastName}".Trim(); 
            } 
        }
    }
} 