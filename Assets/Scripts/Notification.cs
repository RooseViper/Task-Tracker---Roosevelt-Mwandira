using SQLite4Unity3d;
using System.Collections;
using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;

public class Notification : MonoBehaviour
{
    TaskController taskController;
    public string Username { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }

    public string description()
    {
        return string.Format("{0}", Description);
    }

    public string status()
    {
        return string.Format("{0}", Status);
    }

    private void Start()
    {
        taskController = GetComponent<TaskController>();
        this = this;
        var channel = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Generic notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    void Send(string info) {

        var notification = new AndroidNotification();
        notification.Title = "Assigned New Task!";
        notification.Text = info;
        notification.FireTime = System.DateTime.Now.AddSeconds(2);

        AndroidNotificationCenter.SendNotification(notification, "channel_id");
        Debug.Log("Called");
    }

    /// <summary>
    /// Adds new Notification
    /// </summary>
    /// <param name="assignedUser"></param>
    /// <param name="notification"></param>
    public void Add(string assignedUser, string notification)
    {
        string query = @"INSERT INTO Notification(Username, Description, Status) 
                                          VALUES('" + assignedUser +
                                            "', '" + notification +
                                            "', '" + "Unseen" +
                                            "')";
        SQLiteCommand dbCommand = new SQLiteCommand(TaskController.sqliteConnection);
        dbCommand.CommandText = query;
        int i = dbCommand.ExecuteNonQuery();
        if (i == 1)
        {
            Send("Hi, " + assignedUser+"! " + TaskController.loggedInUsername + " Assigned you a new Task.");
        }
    }

    public void GetUserNotifications()
    {
        var notifications = NotificationsTable(TaskController.loggedInUsername);
        foreach (var notification in notifications)
        {
            GameObject notificationsCellObj = Instantiate(taskController.notificationsCellPrefab, taskController.notificationsCellsParent.transform);
            NotifcationCell notificationCell = notificationsCellObj.GetComponent<NotifcationCell>();
            notificationCell.notificationTxt.text = notification.description();
        }
    }

    /// <summary>
    /// Notifies the User that they have new Notifications
    /// </summary>
    public void Notify() {
        var notifications = UnSeenNotifications(TaskController.loggedInUsername);
        int counter = 0;
        foreach (var notification in notifications)
        {
            counter++;
        }
        //No new Unread Notifications
        if (counter == 0)
        {
            MessageState(false);
        }
        //Found Unread Notifications
        else
        {
            MessageState(true);
        }
    }

    /// <summary>
    /// Marks the Notifications as seen/read
    /// </summary>
    public void MarkAsSeen()
    {
        SQLiteCommand cmnd = new SQLiteCommand(TaskController.sqliteConnection);
        cmnd.CommandText = @"UPDATE Notification SET Status = '" + "Seen" +
                                              "' WHERE Username = '" + TaskController.loggedInUsername + "' ";
        int i = cmnd.ExecuteNonQuery();
    }

    /// <summary>
    /// Is the Notification Message visible or not
    /// </summary>
    public void MessageState(bool isEnabled) {
        foreach (GameObject nMessage in taskController.notificationMessages)
        {
            nMessage.SetActive(isEnabled);
        }
    }

    public IEnumerable<Notification> NotificationsTable(string username)
    {
        return TaskController.sqliteConnection.Table<Notification>().Where(x => x.Username == username);
    }

    public IEnumerable<Notification> UnSeenNotifications(string username)
    {
        return TaskController.sqliteConnection.Table<Notification>().Where(x => x.Username == username && x.Status == "Unseen");
    }
}
