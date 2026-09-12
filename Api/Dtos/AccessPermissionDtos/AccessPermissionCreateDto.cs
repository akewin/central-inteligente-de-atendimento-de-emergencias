namespace Api.Dtos
{
  public class AccessPermissionCreateDto
  {
    public required int UserId { get; set; }
    public required int SystemResourceId { get; set; }
  }
}
