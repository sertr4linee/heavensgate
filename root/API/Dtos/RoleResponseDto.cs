namespace API.Dtos
{
    public class RoleResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
    }
}