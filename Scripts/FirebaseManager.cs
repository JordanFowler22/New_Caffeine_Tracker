using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public class FirebaseManager : MonoBehaviour
{
    //Firebase Variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;
    
    //Login Variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;
    
    //Register Variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;
    
    //User Data Variables
    [Header("User Data")] 
    public TMP_InputField caffeineField;
    public TMP_InputField usernameField;
    
    //Password Change Variables
    [Header("Change Password")] 
    public TMP_InputField oldPassField;
    public TMP_InputField newPassField;
    public TMP_InputField verifyPassField;
    public TMP_Text warningChangePassText;
    public TMP_Text confirmChangePassText;
    
    //Username Change Variables
    [Header("Change Username")]
    public TMP_InputField oldUserField;
    public TMP_InputField newUserField;
    public TMP_InputField verifyUserField;
    //public TMP_Text warningChangeUserText;
    //public TMP_Text confirmChangeUserText;
    
    //Delete Account Variables
    [Header("Delete Account")] 
    public TMP_Text warningDeleteText;
    public TMP_Text confirmDeleteText;
    
    //Some Extra Variables
    [Header("Other")] //May not use the variables
    public TMP_Text todayText;
    //public DateTime utcCreated;
    //private float tester = 0f;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {

            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If its available Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        
        //Set the authentication object
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    //Input Field clear functions
    public void ClearLoginField()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }

    public void ClearRegisterFields()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    public void ClearChangePassFields()
    {
        oldPassField.text = "";
        newPassField.text = "";
        verifyPassField.text = "";
    }

    public void ClearChangeUserFields()
    {
        oldUserField.text = "";
        newUserField.text = "";
        verifyUserField.text = "";
    }

    //Login button function
    public void LoginButton()
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    
    //Sign Up button function
    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    
    //Function to sign out of Firebase
    public void SignOutButton()
    {
        auth.SignOut();
        Debug.Log("User Logged Out Successfully");
        UIManager.instance.LoginScreen();
        ClearLoginField();
        ClearRegisterFields();
    }

    //Function to save the data to Firebase
    public void SaveDataButton()
    {
        StartCoroutine(UpdateUsernameAuth(usernameField.text));
        StartCoroutine(UpdateUsernameDatabase(usernameField.text));

        StartCoroutine(UpdateCaffeine(int.Parse(caffeineField.text)));
        //StartCoroutine(UpdateDatabaseTime(tester));
    }

    //function to change the password for account with Firebase
    public void ChangePasswordButton()
    {
        StartCoroutine(ChangePassword(newPassField.text));
    }

    //Function to delete the logged in account
    public void DeleteAccountButton()
    {
        StartCoroutine(DeleteAccount());
    }

    //Function to quit the game
    public void QuitApp()
    {
        Application.Quit();
    }


    //All IEnumerators used for different functions
    
    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function giving it email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        /*Wait until the task is done*/
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there's errors they are handled here
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.IsCompleted}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError) firebaseEx.ErrorCode;

            string message = "Login Failed";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "User Not Found";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = " ";
            confirmLoginText.text = "Logged in";
            StartCoroutine(LoadUserData());
            
            yield return new WaitForSeconds(1.5f);

            usernameField.text = User.DisplayName;
            UIManager.instance.UserDataScreen();//Change to user data UI
            confirmLoginText.text = "";
            ClearLoginField();
            ClearRegisterFields();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username is blank show a warning
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password and verify passwords don't match show a warning
            warningRegisterText.text = "Passwords Do Not Match";
        }
        else
        {
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task is completed
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors they are handled here
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError) firebaseEx.ErrorCode;

                string message = "Sign Up Failed";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email already in use";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile {DisplayName = _username};
                    
                    //Call the Firebase auth update user profile funtion giving it the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task is done
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors they are handled here
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError) firebaseEx.ErrorCode;
                        warningRegisterText.text = "Username Set Failed";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        
                        UIManager.instance.LoginScreen();
                        
                        warningRegisterText.text = "";
                        ClearLoginField();
                        ClearRegisterFields();
                    }
                }
            }
        }
    }

    private IEnumerator UpdateUsernameAuth(string _username)
    {
        //Create a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _username};
        
        //Call the firebase auth update user profile function
        var ProfileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task is done
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            //Auth username is now updated
        }
    }

    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database is now updated
        }
    }

    /*
    private IEnumerator UpdateDatabaseTime(float _timestamp)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("timestamp").SetValueAsync(_timestamp);
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            var timeMap = Firebase.Database.ServerValue.Timestamp.ToString();
            Debug.Log(timeMap);
            
            Debug.Log("DONE");
        }
    }
    */

    private IEnumerator UpdateCaffeine(int _amount)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("amount").SetValueAsync(_amount);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //amount is now updated

            todayText.text = $"Today: {caffeineField.text}";
            
            
            
            Debug.Log("Amount Stored");
        }
    }
    
    private IEnumerator LoadUserData()
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();
        
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            caffeineField.text = "0";
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            caffeineField.text = snapshot.Child("amount").Value.ToString();
        }
    }
    
    private IEnumerator DeleteAccount()
    {
        if (auth.CurrentUser.Equals(null))
        {
            warningDeleteText.text = "User Not Logged In";
        }
        else
        {
            var AccountTask = auth.CurrentUser.DeleteAsync();
        
            yield return new WaitUntil(predicate: () => AccountTask.IsCompleted);
        
            if (AccountTask.Exception != null)
            {
                //If there are errors they are handled here
                Debug.LogWarning(message: $"Failed to register task with {AccountTask.Exception}");
                FirebaseException firebaseEx = AccountTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError) firebaseEx.ErrorCode;

                string message = "Account Deletion Failed";
            
                warningDeleteText.text = message;
            }
            else
            {
                //Account has been deleted
                //Now return to start screen

                confirmDeleteText.text = "Account Deleted";
                warningDeleteText.text = "";
            
                yield return new WaitForSeconds(1.5f);
            
                UIManager.instance.StartScreen();

                confirmDeleteText.text = "";
                warningDeleteText.text = "";
            }
        }
    }

    private IEnumerator ChangePassword(string _password)
    {
        if (_password == "")
        {
            //If the username is blank show a warning
            warningRegisterText.text = "Missing Password";
        }
        else if (newPassField.text != verifyPassField.text)
        {
            //If the password and verify passwords don't match show a warning
            warningChangePassText.text = "Passwords Do Not Match";
        }
        else
        {
            var PasswordTask = User.UpdatePasswordAsync(_password);
            //Wait until the task is completed
            yield return new WaitUntil(predicate: () => PasswordTask.IsCompleted);
            
            if (PasswordTask.Exception != null)
            {
                //If there are errors they are handled here
                Debug.LogWarning(message: $"Failed to register task with {PasswordTask.Exception}");
                FirebaseException firebaseEx = PasswordTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError) firebaseEx.ErrorCode;

                string message = "Password Change Failed";
                switch (errorCode)
                {
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                }
                warningChangePassText.text = message;
            }
            else
            {
                 //Password is now changed
                 //Now return to settings screen

                 warningChangePassText.text = "";
                 confirmChangePassText.text = "Password Changed";
                 
                 yield return new WaitForSeconds(1.5f);
                 
                 UIManager.instance.Settings();

                 confirmChangePassText.text = "";
                 warningChangePassText.text = "";
                 ClearChangePassFields();
            }
        }
    }

    /*
    private IEnumerator ChangeUsername(string _username)
    {
        if (_username == "")
        {
            //If the username is blank show a warning
            warningChangeUserText.text = "Missing Username";
        }
        else if (newUserField.text != verifyUserField.text)
        {
            //If the password and verify passwords don't match show a warning
            warningRegisterText.text = "Passwords Do Not Match";
        }
        else
        {
            var UsernameTask = User.UpdatePasswordAsync(_username);
            //Wait until the task is completed
            yield return new WaitUntil(predicate: () => UsernameTask.IsCompleted);
            
            if (UsernameTask.Exception != null)
            {
                //If there are errors they are handled here
                Debug.LogWarning(message: $"Failed to register task with {UsernameTask.Exception}");
                FirebaseException firebaseEx = UsernameTask.Exception.GetBaseException() as FirebaseException;
                warningChangeUserText.text = "Username Change Failed";
            }
            else
            {
                 //username is now changed
                 //Now return to settings screen
                 
                 UIManager.instance.Settings();
                 
                 warningChangePassText.text = "";
                 ClearChangeUserFields();
            }
        }
    }
    */
}
