namespace RaceDayAPI.Models;

public class Result
{
    public int ResultID { get; set; }
    public int EnrollmentID { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public decimal? Pace { get; set; }

    public DateTime ResultDate { get; set; } = DateTime.Now;
    public Enrollment Enrollment { get; set; } = null!;
}
