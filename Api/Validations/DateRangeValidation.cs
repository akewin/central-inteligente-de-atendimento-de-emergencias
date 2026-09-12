using System;
using Api.Middlewares;

namespace Api.Validations
{
  public static class ValidateDateRange
  {
    public static void EnsureValidPeriod(DateTime? startDate, DateTime? endDate)
    {
      if (startDate.HasValue && startDate.Value > DateTime.UtcNow)
      {
        throw new AppException("Data inicial não pode ser uma data futura.");
      }

      if (endDate.HasValue && startDate.HasValue && endDate.Value < startDate.Value)
      {
        throw new AppException("Data final não pode ser anterior à data inicial.");
      }
    }
  }
}
