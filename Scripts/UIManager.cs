using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startUI;
    public GameObject loginUI;
    public GameObject signupUI;
    public GameObject mainUI;
    public GameObject calendarUI;
    public GameObject settingsUI;
    public GameObject changePasswordUI;
    public GameObject changeUsernameUI;
    public GameObject DeleteAccountUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //Functions to change the login screen UI

    public void ClearScreen() //Turn off all screens
    {
        startUI.SetActive(false);
        loginUI.SetActive(false);
        signupUI.SetActive(false);
        mainUI.SetActive(false);
        calendarUI.SetActive(false);
        settingsUI.SetActive(false);
        changePasswordUI.SetActive(false);
        changeUsernameUI.SetActive(false);
        DeleteAccountUI.SetActive(false);
    }

    public void StartScreen() //Start UI
    {
        ClearScreen();
        startUI.SetActive(true);
    }
    
    public void LoginScreen() //Login UI
    {
        ClearScreen();
        loginUI.SetActive(true);
    }
    
    public void RegisterScreen() // Sign Up UI
    {
        ClearScreen();
        signupUI.SetActive(true);
    }

    public void UserDataScreen() //Main UI
    {
        ClearScreen();
        mainUI.SetActive(true);
    }
    
    public void Calendar() //Calendar UI
    {
        ClearScreen();
        calendarUI.SetActive(true);
    }
    
    public void Settings() //Settings UI
    {
        ClearScreen();
        settingsUI.SetActive(true);
    }

    public void ChangePassword() //Change Password UI
    {
        ClearScreen();
        changePasswordUI.SetActive(true);
    }
    
    public void ChangeUsername() //Change Username UI
    {
        ClearScreen();
        changeUsernameUI.SetActive(true);
    }

    public void DeleteAccount() //Delete Account UI
    {
        ClearScreen();
        DeleteAccountUI.SetActive(true);
    }
}
