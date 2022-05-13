
using SQLite4Unity3d;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class User : MonoBehaviour {

    public string Username { get; set; }

    public string Password { get; set; }
    public string Email { get; set; }

    public string username()
    {
        return string.Format("{0}", Username);
    }

    public string password()
    {
        return string.Format("{0}", Password);
    }


}
