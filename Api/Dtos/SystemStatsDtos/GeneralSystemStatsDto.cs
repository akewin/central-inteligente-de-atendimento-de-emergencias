namespace Api.Dtos
{
  public class GeneralSystemStatsDto
  {
    public int UsersCount { get; set; }
    public int SystemResourcesCount { get; set; }
    public int MonthlyReportsCount { get; set; }
    public int ActiveOccurrencesCount { get; set; }
    public int DispatchedOccurrencesCount { get; set; }
    public int FinalizedOccurrencesCount { get; set; }
    public int CancelledOccurrencesCount { get; set; }

    public string MonthlyReportsCountReference { get; set; } = string.Empty;
  }
}
