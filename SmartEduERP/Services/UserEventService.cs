using SmartEduERP.Data.Models;

namespace SmartEduERP.Services;

public class UserEventService
{
    // Events for real-time notifications
    public event Action<Student>? OnStudentAdded;
    public event Action<Teacher>? OnTeacherAdded;
    public event Action<UserAccount>? OnUserAdded;

    // Methods to trigger events
    public void NotifyStudentAdded(Student student)
    {
        OnStudentAdded?.Invoke(student);
    }

    public void NotifyTeacherAdded(Teacher teacher)
    {
        OnTeacherAdded?.Invoke(teacher);
    }

    public void NotifyUserAdded(UserAccount user)
    {
        OnUserAdded?.Invoke(user);
    }
}