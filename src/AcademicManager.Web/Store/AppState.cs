namespace AcademicManager.Web.Store;

public class AppState
{
    public bool IsSidebarCollapsed { get; set; }
    public string CurrentUser { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public bool IsAuthenticated { get; set; }
    public List<string> Notifications { get; set; } = new();
}

public class AppStateContainer
{
    private AppState _state = new();

    public AppState State => _state;

    public event Action? StateChanged;

    public void ToggleSidebar()
    {
        _state.IsSidebarCollapsed = !_state.IsSidebarCollapsed;
        NotifyStateChanged();
    }

    public void SetSidebarCollapsed(bool collapsed)
    {
        _state.IsSidebarCollapsed = collapsed;
        NotifyStateChanged();
    }

    public void SetUser(int userId, string username, string role)
    {
        _state.UserId = userId;
        _state.CurrentUser = username;
        _state.UserRole = role;
        _state.IsAuthenticated = true;
        NotifyStateChanged();
    }

    public void ClearUser()
    {
        _state = new AppState();
        NotifyStateChanged();
    }

    public void AddNotification(string message)
    {
        _state.Notifications.Add(message);
        NotifyStateChanged();
    }

    public void ClearNotifications()
    {
        _state.Notifications.Clear();
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
