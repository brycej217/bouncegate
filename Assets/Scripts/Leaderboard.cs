 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    int boardID = 7140;

    private TextMeshProUGUI Names;
    private TextMeshProUGUI Scores;

    private void Awake()
    {
        Names = GameObject.Find("/Canvas/LeaderboardContainer/Names").GetComponent<TextMeshProUGUI>();
        Scores = GameObject.Find("/Canvas/LeaderboardContainer/Scores").GetComponent<TextMeshProUGUI>();
    }
    
    public void SetPlayerName(string name)
    {
        LootLockerSDKManager.SetPlayerName(name, (response) =>
        {
            if (response.success)
            {
                Debug.Log("name set success");
            }
            else
            {
                Debug.Log("name set fail" + response.Error);
            }
        });
    }

    public IEnumerator SubmitScoreRoutine(int scoreToUpload) {
        bool fin = false;
        string playerID;
  
        playerID = PlayerPrefs.GetString("PlayerID");
        LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, boardID, (response) =>
        {
            if (response.success)
            {
                Debug.Log("upload success");
                fin = true;
            }
            else {
                Debug.Log("failure to upload" + response.Error);
                fin = true; 
            }
        });
        yield return new WaitWhile(() => fin == false);
    }

    public IEnumerator FetchScores() {
        bool done = false;
        LootLockerSDKManager.GetScoreList(boardID, 10, 0, (response) =>
        {
            if (response.success) {
                string tempPlayerNames = "";
                string tempPlayerScores = "";


                LootLockerLeaderboardMember[] members = response.items;
                
                for(int i = 0; i < members.Length; i++) {
                    tempPlayerNames += members[i].rank + ". ";
                    if(members[i].player.name != "") {
                        tempPlayerNames += members[i].player.name;
                    }
                    else {
                        tempPlayerNames += members[i].player.id;
                    }
                    tempPlayerScores += members[i].score + "\n";
                    tempPlayerNames += "\n";
                }
                
               done = true;

                Names.text = tempPlayerNames;
                Scores.text = tempPlayerScores;
            }
            else {
                Debug.Log("Fetch failed" + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }
}
