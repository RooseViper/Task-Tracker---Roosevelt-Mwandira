using SQLite4Unity3d;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Task : MonoBehaviour
{
    public InputField titleTxt, descTxt, commentTxt;
    public Text usernameTxt, dayTxt, monthTxt, yearTxt, titleValidator;
    public Dropdown userDropList;
    public string Username { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Day { get; set; }
    public string Month { get; set; }
    public string Year { get; set; }
    public string Comment { get; set; }
    public string Status { get; set; }

    TaskController taskController;
    Notification notification;
    Coroutine titleCRT;
    public bool editMode = false;

    string toggledStatus = "";  //Stores the on and off status of a task
    public string title()
    {
        return string.Format("{0}", Title);
    }

    public string description()
    {
        return string.Format("{0}", Description);
    }

    public string day()
    {
        return string.Format("{0}", Day);
    }

    public string month()
    {
        return string.Format("{0}", Month);
    }

    public string year()
    {
        return string.Format("{0}", Year);
    }

    public string comment()
    {
        return string.Format("{0}", Comment);
    }

    public string status()
    {
        return string.Format("{0}", Status);
    }
    private void Start()
    {
        taskController = GetComponent<TaskController>();
        notification = taskController.GetComponent<Notification>();
    }

    public void Assign()
    {
        usernameTxt.text = userDropList.options[userDropList.value].text;
    }

    public void Create()
    {
        if (!editMode)
        {
            if (ReturnedTask(titleTxt.text).Equals(titleTxt.text))
            {
                titleValidator.enabled = true;
                if (titleCRT != null)
                {
                    StopCoroutine(titleCRT);
                }
                titleCRT = null;
                titleCRT = StartCoroutine(PopupError(titleValidator));
            }
            else
            {
                string query = @"INSERT INTO Task(Username, Title, Description, Day, Month, Year, Comment, Status) 
                                          VALUES('" + usernameTxt.text +
                                           "', '" + titleTxt.text +
                                           "', '" + descTxt.text +
                                           "', '" + dayTxt.text +
                                           "', '" + monthTxt.text +
                                           "', '" + yearTxt.text +
                                           "', '" + commentTxt.text +
                                           "', '" + "In Progress" +
                                           "')";
                SQLiteCommand dbCommand = new SQLiteCommand(TaskController.sqliteConnection);
                dbCommand.CommandText = query;
                int i = dbCommand.ExecuteNonQuery();
                if (i == 1)
                {
                    taskController.LoadTasks();
                    AddNotification();
                    taskController.ActivatePage(TaskController.PagePanel.TASKS);
                }
            }
        }
    }

    /// <summary>
    /// Adds Notifcation when different user is assigned a Task
    /// </summary>
    void AddNotification() {
        if (usernameTxt.text != TaskController.loggedInUsername)
        {
            notification.Add(usernameTxt.text, TaskController.loggedInUsername + " Assigned you a Task(" + titleTxt.text + 
                                                                                 ") at " + DateTime.Now);
        }
    }

    public void Clear() {
        titleTxt.text = ""; descTxt.text = "";
        dayTxt.text = "1";
        monthTxt.text = "January";
        yearTxt.text = "2022";
        commentTxt.text = "";
    }

    public void Retrieve()
    {
        var tasks = TaskTable(TaskController.loggedInUsername, taskController.taskToUpdate);
   
        foreach (var task in tasks)
        {
            titleTxt.text = task.title();
            descTxt.text = task.description();
            dayTxt.text = task.day();
            monthTxt.text = task.month();
            yearTxt.text = task.year();
            commentTxt.text = task.comment();
       
            //if (task.status().Equals("In Progress"))
            //{
            //    taskCell.toggle.isOn = false;
            //}
            //else
            //{
            //    taskCell.toggle.isOn = true;
            //}
        }
    }

    public void Edit()
    {
        if (editMode)
        {
            SQLiteCommand cmnd = new SQLiteCommand(TaskController.sqliteConnection);
            cmnd.CommandText = @"UPDATE Task SET Title = '" + titleTxt.text +
                                             "', Username = '" + usernameTxt.text +
                                             "', Description = '" + descTxt.text +
                                             "', Day = '" +dayTxt.text +
                                             "', Month = '" + monthTxt.text+
                                             "', Year = '" + yearTxt.text +
                                             "', Comment = '" + commentTxt.text +
                                             "' WHERE Username = '" + TaskController.loggedInUsername + "' AND " +
                                            "   Title = '" + taskController.taskToUpdate + "' ";
            int i = cmnd.ExecuteNonQuery();
            if (i == 1)
            {
                taskController.LoadTasks();
                AddNotification();
                taskController.ActivatePage(TaskController.PagePanel.TASKS);
            }
        }
    }
    /// <summary>
    /// Toggles and Saves the Task on and off
    /// </summary>
    public void Toggle(bool isOn, string title)
    {
        if (isOn)
        {
            toggledStatus = "Complete";
        }
        else
        {
            toggledStatus = "In Progress";
        }
        SQLiteCommand cmnd = new SQLiteCommand(TaskController.sqliteConnection);
        cmnd.CommandText = @"UPDATE Task SET Status = '" + toggledStatus +
                                         "' WHERE Username = '" + TaskController.loggedInUsername + "' AND " +
                                        "   Title = '" + title + "' ";
        int i = cmnd.ExecuteNonQuery();
    }

    public void Delete(string title)
    {
        SQLiteCommand cmnd = new SQLiteCommand(TaskController.sqliteConnection);
        cmnd.CommandText = @"DELETE FROM Task WHERE Username = '" + TaskController.loggedInUsername + "' AND " +
                                                   "Title = '" + title + "' ";
        int i = cmnd.ExecuteNonQuery();
        if (i == 1)
        {
            taskController.ClearCells(taskController.taskCellsParent);
            taskController.LoadTasks();
            StartCoroutine(taskController.Refresh());
            taskController.ActivatePage(TaskController.PagePanel.TASKS);
        }
    }

 
    IEnumerator PopupError(Text text)
    {
        yield return new WaitForSeconds(2.5f);
        text.enabled = false;
    }
    /// <summary>
    /// Sets the list of Tasks in the cells in the Tasks Panel
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public void GetUserTasks()
    {
        var tasks = AllTasksTable(TaskController.loggedInUsername);
        foreach (var task in tasks)
        {
            GameObject taskCellObj = Instantiate(taskController.taskCellPrefab,taskController.taskCellsParent.transform);
            TaskCell taskCell = taskCellObj.GetComponent<TaskCell>();
            taskCell.taskNameTxt.text = task.title();
            if (task.status().Equals("In Progress"))
            {
                taskCell.toggle.isOn = false;
            }
            else
            {
                taskCell.toggle.isOn = true;
            }
        }
    }
    public IEnumerable<Task> AllTasksTable(string username)
    {
        return TaskController.sqliteConnection.Table<Task>().Where(x => x.Username == username);
    }

    public IEnumerable<Task> TaskTable(string username, string title)
    {
        return TaskController.sqliteConnection.Table<Task>().Where(x => x.Username == username && x.Title == title);
    }

    private string ReturnedTask(string name)
    {
        string value = "";
        var tasks = GetTasks(name);
        foreach (var task in tasks)
        {
            value = task.title();
        }
        return value;
    }

    public IEnumerable<Task> GetTasks(string title)
    {
        return TaskController.sqliteConnection.Table<Task>().Where(x => x.Title == title);
    }
}
