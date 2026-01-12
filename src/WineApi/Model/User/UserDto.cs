namespace WineApi.Model.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public string? LastOn { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class AddUpdateUser
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public bool IsAdmin { get; set; }
    }
}
