using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PubNubAPI;
using UnityEngine.UI;
//using SimpleJSON;

public class MyClass
{
    public string username;
    public string score;
    public string refresh;
}

public class leaderboard : MonoBehaviour
{
    public static PubNub pubnub;
    //public Text Line1;
    //public Text Line2;
    //public Text Line3;
    //public Text Line4;
    //public Text Line5;
    //public Text Score1;
    //public Text Score2;
    //public Text Score3;
    //public Text Score4;
    //public Text Score5;

    public Button SubmitButton;
    public InputField FieldUsername;
    public InputField FieldScore;
    //public Object[] tiles = {}
    // Use this for initialization

    public GameObject prefabRow;
    public Transform placePrefabRow;
    void Start()
    {
        Button btn = SubmitButton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);

        // Use this for initialization
        PNConfiguration pnConfiguration = new PNConfiguration();
        pnConfiguration.PublishKey = "pub-c-f07ff9f1-56e8-4d21-8792-c0d459444838";
        pnConfiguration.SubscribeKey = "sub-c-81d64bf1-ee92-4006-9c53-8a576c8a4046";

        pnConfiguration.LogVerbosity = PNLogVerbosity.BODY;
        pnConfiguration.UUID = Random.Range(0f, 999999f).ToString();

        pubnub = new PubNub(pnConfiguration);
        Debug.Log(pnConfiguration.UUID);

        MyClass fireRefreshObject = new MyClass();
        fireRefreshObject.refresh = "new user refresh";
        string firerefreshobject = JsonUtility.ToJson(fireRefreshObject);
        pubnub.Fire() // This will trigger the leaderboard to refresh so it will display for a new user. 
            .Channel("submit_score")
            .Message(firerefreshobject)
            .Async((result, status) => {
                if (status.Error)
                {
                    Debug.Log(status.Error);
                    Debug.Log(status.ErrorData.Info);
                }
                else
                {
                    Debug.Log(string.Format("Fire Timetoken: {0}", result.Timetoken));
                }
            });

        pubnub.SubscribeCallback += (sender, e) => {
            SubscribeEventEventArgs mea = e as SubscribeEventEventArgs;
            if (mea.Status != null)
            {
            }
            if (mea.MessageResult != null)
            {
                Dictionary<string, object> msg = mea.MessageResult.Payload as Dictionary<string, object>;

                string[] strArr = msg["username"] as string[];
                string[] strScores = msg["score"] as string[];

                //show new
                foreach(Transform t in placePrefabRow) { Destroy(t.gameObject); }
                for(int i = 0;i<strArr.Length;i++)
                {
                    GameObject g = Instantiate(prefabRow, placePrefabRow);
                    entry set = g.GetComponent<entry>();
                    set.txtName.text = strArr[i];
                    set.txtScore.text = strScores[i];
                }


                //show old
                //int usernamevar = 1;
                //foreach (string username in strArr)
                //{
                //    string usernameobject = "Line" + usernamevar;
                //    GameObject.Find(usernameobject).GetComponent<Text>().text = usernamevar.ToString() + ". " + username.ToString();
                //    usernamevar++;
                //    Debug.Log(username);
                //}

                //int scorevar = 1;
                //foreach (string score in strScores)
                //{
                //    string scoreobject = "Score" + scorevar;
                //    GameObject.Find(scoreobject).GetComponent<Text>().text = "Score: " + score.ToString();
                //    scorevar++;
                //    Debug.Log(score);
                //}
            }
            if (mea.PresenceEventResult != null)
            {
                Debug.Log("In Example, SubscribeCallback in presence" + mea.PresenceEventResult.Channel + mea.PresenceEventResult.Occupancy + mea.PresenceEventResult.Event);
            }
        };
        pubnub.Subscribe()
            .Channels(new List<string>() {
                "leaderboard_scores"
            })
            .WithPresence()
            .Execute();
    }

    void TaskOnClick()
    {
        var usernametext = FieldUsername.text;// this would be set somewhere else in the code
        var scoretext = FieldScore.text;
        MyClass myObject = new MyClass();
        myObject.username = FieldUsername.text;
        myObject.score = FieldScore.text;
        string json = JsonUtility.ToJson(myObject);

        pubnub.Publish()
            .Channel("submit_score")
            .Message(json)
            .Async((result, status) => {
                if (!status.Error)
                {
                    Debug.Log(string.Format("Publish Timetoken: {0}", result.Timetoken));
                }
                else
                {
                    Debug.Log(status.Error);
                    Debug.Log(status.ErrorData.Info);
                }
            });
        //Output this to console when the Button is clicked
        Debug.Log("You have clicked the button!");
    }

}