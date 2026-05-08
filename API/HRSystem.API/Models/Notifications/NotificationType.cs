namespace HRSystem.API.Models.Notifications;

public enum NotificationType
{
    LeaveSubmitted = 1,
    LeaveApproved = 2,
    LeaveRejected = 3,
    OvertimeDetected = 4,
    OvertimeApproved = 5,
    NewAnnouncement = 6,
    OnboardingTaskAssigned = 7,
    OnboardingTaskOverdue = 8,
    ReviewCycleStarted = 9,
    ReviewPending = 10,
    DocumentExpiring = 11,
    EmployeeProfileChanged = 12,
    TaskAssigned = 13
}
