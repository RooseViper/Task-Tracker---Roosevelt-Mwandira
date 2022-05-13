using SQLite4Unity3d;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UserManager : MonoBehaviour
{
    public TaskController taskController;
    public Text usernameTxt, firstNameTxt, lastNameTxt, emailTxt, signInUsernameTxt,
                signUpValidator, signInUsrnameValidator, signInPasswordValidator, passwordValidator ;
    public InputField passwrdInput, cnfrmpasswrdInput, signInPasswordInput;
    private SQLiteConnection _connection;
    Coroutine signInUsernameCRT, signInPasswordCRT, signUpCRT, confrmPswdCRT;
    public string Username { get; set; }

    public string Password { get; set; }
    public string Email { get; set; }

    public void DataService(string DatabaseName)
    {
#if UNITY_EDITOR
        var dbPath = string.Format(@"Assets/StreamingAssets/{0}", DatabaseName);
#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#elif UNITY_WINRT
		var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
		
#elif UNITY_STANDALONE_OSX
		var loadDb = Application.dataPath + "/Resources/Data/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
#else
	var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
	// then save to Application.persistentDataPath
	File.Copy(loadDb, filepath);

#endif

            Debug.Log("Database written");
        }

        var dbPath = filepath;
#endif
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        TaskController.sqliteConnection = _connection;
        Debug.Log("Final PATH: " + dbPath);
        //    testText.text = dbPath;
    }

   
    void Start()
    {
        DataService("task_tracker.db");

    }


    private string ReturnedUsername(string name)
    {
        string value = "";
        var users = GetName(name);
        foreach (var user in users)
        {
            value = user.username();
        }

        return value;
    }

    private string ReturnedPasswords(string password)
    {
        string value = "";
        var passwords = GetPassword(password);
        foreach (var pass in passwords)
        {
            value = pass.password();
        }

        return value;
    }

    private string AthenticatedUser(string username, string password)
    {
        string value = "";
        var users = GetAuthentication(username, password);
        foreach (var user in users)
        {
            value = user.username();
        }

        return value;
    }

    public IEnumerable<User> GetName(string username)
    {
        return _connection.Table<User>().Where(x => x.Username == username);
    }

    public IEnumerable<User> GetPassword(string password)
    {
        return _connection.Table<User>().Where(x => x.Password == password);
    }

    public IEnumerable<User> GetAuthentication(string username, string password)
    {
        return _connection.Table<User>().Where(x => x.Username == username && x.Password == password);
    }

    public List<string> usersList()
    {
        List<string> value = new List<string>();
        var users = GetUsers();
        foreach (var user in users)
        {
            value.Add(user.username());
        }
        return value;
    }

    public IEnumerable<User> GetUsers()
    {
        return _connection.Table<User>();
    }


    public void SignIn()
    {
        if (string.IsNullOrEmpty(signInUsernameTxt.text) || string.IsNullOrEmpty(signInPasswordInput.text))
            return;
        
        if (!ReturnedUsername(signInUsernameTxt.text).Equals(signInUsernameTxt.text))
        {
            signInUsrnameValidator.enabled = true;
            if (signInUsernameCRT != null)
            {
                StopCoroutine(signInUsernameCRT);
            }
            signInUsernameCRT = null;
            signInUsernameCRT = StartCoroutine(PopupError(signInUsrnameValidator));
        }
        else if ((ReturnedUsername(signInUsernameTxt.text).Equals(signInUsernameTxt.text) && 
                 !ReturnedPasswords(signInPasswordInput.text).Equals(signInPasswordInput.text)) ||
                 (AthenticatedUser(signInUsernameTxt.text, signInPasswordInput.text) != signInUsernameTxt.text))
        {
            signInPasswordValidator.enabled = true;
            if (signInPasswordCRT != null)
            {
                StopCoroutine(signInPasswordCRT);
            }
            signInPasswordCRT = null;
            signInPasswordCRT = StartCoroutine(PopupError(signInPasswordValidator));
        }
        else if(AthenticatedUser(signInUsernameTxt.text, signInPasswordInput.text).Equals(signInUsernameTxt.text))
        {
            TaskController.loggedInUsername = signInUsernameTxt.text;
            taskController.LoadTasks();
            taskController._Notification.Notify();
            taskController.ActivatePage(TaskController.PagePanel.TASKS);
        }
    }

    public void SignUp()
    {
        if (ReturnedUsername(usernameTxt.text).Equals(usernameTxt.text))
        {
            signUpValidator.enabled = true;
            if (signUpCRT != null)
            {
                StopCoroutine(signUpCRT);
            }
            signUpCRT = null;
            signUpCRT = StartCoroutine(PopupError(signUpValidator));
        }
        else if (passwrdInput.text != cnfrmpasswrdInput.text)
        {
            passwordValidator.enabled = true;
            if (confrmPswdCRT != null)
            {
                StopCoroutine(confrmPswdCRT);
            }
            confrmPswdCRT = null;
            confrmPswdCRT = StartCoroutine(PopupError(passwordValidator));
        }
        else 
        {
            string query = @"INSERT INTO User(Username, Password, Firstname, Lastname, Email) 
                                          VALUES('" + usernameTxt.text +
                                       "', '" + passwrdInput.text +
                                       "', '" + firstNameTxt.text +
                                       "', '" + lastNameTxt.text +
                                       "', '" + emailTxt.text +
                                       "')";
            SQLiteCommand dbCommand = new SQLiteCommand(_connection);
            dbCommand.CommandText = query;
            int i = dbCommand.ExecuteNonQuery();
            if (i == 1)
            {

                taskController.ActivatePage(TaskController.PagePanel.SIGN_IN);
            }
        }
    }

    IEnumerator PopupError(Text text) {
        yield return new WaitForSeconds(2.5f);
        text.enabled = false;
    }
}
