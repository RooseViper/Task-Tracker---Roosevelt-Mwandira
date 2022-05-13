using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskCell : MonoBehaviour
{
    public Text taskNameTxt;
    public Toggle toggle;
    public Button editButton;
    public Button deleteButton;
    TaskController taskController;
    Task task;

    private void Start()
    {
        taskController = FindObjectOfType<TaskController>();
        task = taskController.GetComponent<Task>();
        editButton.onClick.AddListener(Edit);
        deleteButton.onClick.AddListener(Delete);
    }

    void Edit() {
        taskController.ClearCells(taskController.taskCellsParent);
        taskController.taskToUpdate = taskNameTxt.text;
        taskController.OpenAddUpdatePage(true);
    }

    void Delete()
    {
        task.Delete(taskNameTxt.text);
    }

    public void ToggleTask() {
        GetTask();
    }

    void GetTask() {
        if (task != null)
        {
            task.Toggle(toggle.isOn, taskNameTxt.text);
        }
    }
  
}
