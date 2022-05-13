using SQLite4Unity3d;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TaskController : MonoBehaviour
{
    public enum PagePanel { 
        SIGN_IN,
        SIGN_UP,
        TASKS,
        ADD_UPDATE,
        NOTIFICATIONS
    }
    public PagePanel currentPagePanel;
    public Page[] pages;
    public UserManager userManager;
    public GameObject taskCellsParent, taskCellPrefab, dummyTaskCellPrefab;
    public GameObject notificationsCellsParent, notificationsCellPrefab, dummynotificationsCellPrefab;
    public GameObject[] notificationMessages;
    // Start is called before the first frame update
    Task task;
    Notification notification;
    public static SQLiteConnection sqliteConnection;
    public static string loggedInUsername;

    public string taskToUpdate { get; set; }
    public Notification _Notification { get => notification; set => notification = value; }

    private void Start()
    {
        task = GetComponent<Task>();
        notification = GetComponent<Notification>();
    }

    public void Back() {
        switch (currentPagePanel)
        {
            case PagePanel.SIGN_UP:
                ActivatePage(PagePanel.SIGN_IN);
                break;
            case PagePanel.TASKS:
                ActivatePage(PagePanel.SIGN_IN);
                break;
            case PagePanel.ADD_UPDATE:
                ActivatePage(PagePanel.TASKS);
                break;
            case PagePanel.NOTIFICATIONS:
                ActivatePage(PagePanel.TASKS);
                break;
            default:
                break;
        }
    }

    public void LoadTasks() {
        task.GetUserTasks();
        AlignCells(taskCellsParent, dummyTaskCellPrefab);
    }

    public void LoadNotifications()
    {
        notification.GetUserNotifications();
        notification.MessageState(false);
        AlignCells(notificationsCellsParent, dummynotificationsCellPrefab);
        notification.MarkAsSeen();
        ActivatePage(PagePanel.NOTIFICATIONS);
    }
    /// <summary>
    /// Refreshes Tasks Panel after Deleting a Task
    /// </summary>
    /// <returns></returns>
    public IEnumerator Refresh()
    {
        yield return new WaitForEndOfFrame();
        AlignCells(taskCellsParent, dummyTaskCellPrefab); 
    }
    /// <summary>
    /// Aligns the cells to be aligned in a presentable way for the users
    /// </summary>
    void AlignCells(GameObject itemParent, GameObject dummyCellPrefab) {
        int dummyCells = 6 - itemParent.transform.childCount;
        if (dummyCells < 6)
        {
            for (int i = 0; i < dummyCells; i++)
            {
                Instantiate(dummyCellPrefab, itemParent.transform);
            }
        }
    }


    public void ClearCells(GameObject cellsParent)
    {
        Cell[] cells = cellsParent.GetComponentsInChildren<Cell>();
        foreach (Cell cell in cells)
        {
            Destroy(cell.gameObject);
        }
    }


    public void OpenSignUpPage() {
        ActivatePage(PagePanel.SIGN_UP);
    }

    public void OpenTasksPage() {

        ActivatePage(PagePanel.TASKS);
    }
    /// <summary>
    /// Opens the Create & Edit Task Page
    /// </summary>
    /// <param name="isEditing"></param>
    public void OpenAddUpdatePage(bool isEditing)
    {
        task.editMode = isEditing;
        List<string> users = userManager.usersList().Distinct().ToList();
        task.userDropList.ClearOptions();
        task.userDropList.AddOptions(users);
        task.userDropList.value = task.userDropList.options.FindIndex(option=> option.text == loggedInUsername);
        if (isEditing)
        {
            task.Retrieve();
        }
        else
        {
            task.Clear();
        }
        task.usernameTxt.text = loggedInUsername;
        ActivatePage(PagePanel.ADD_UPDATE);
    }

    public void ActivatePage(PagePanel pagePanel) {
        foreach (Page item in pages)
        {
            item.gameObject.SetActive(false);
        }
        Page page = System.Array.Find(pages, pg => pg.page == pagePanel);
        page.gameObject.SetActive(true);
        currentPagePanel = pagePanel;
    }
}
