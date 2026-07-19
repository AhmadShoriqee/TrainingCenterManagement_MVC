using System;

namespace TrainingCenterManagement_MVC.ViewModels
{
    public class PresenceOverviewRow
    {
        public Guid LectureId { get; set; }
        public string LectureTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime LectureDate { get; set; }
        public bool IsUpcoming { get; set; }
        public bool IsMarked { get; set; }
        public int PresentCount { get; set; }
        public int TotalMarked { get; set; }
    }
}
