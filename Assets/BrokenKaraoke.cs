using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;
public class BrokenKaraoke : MonoBehaviour {
    public KMSelectable[] keys;
    public KMSelectable buzzer;
    public TextMesh[] karakoeTexts;
    public TextMesh solveText;
    public BrokenKaraokeData data;
    public GameObject karakoe;
    public GameObject keyboard;
    int songIndex;
    string karaokeString;
    string expectedString;
    string inputtedString;
    int buzzerState;
    public KMBombModule module;
    public KMAudio sound;
    int moduleId;
    static int moduleIdCounter = 1;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach(KMSelectable key in keys)
        {
            KMSelectable pressedKey = key;
            pressedKey.OnInteract += delegate { pressKey(key); return false; };
        }
        buzzer.OnInteract += delegate () { pressBuzzer(); return false; };
    }
    void Start () {
        keyboard.SetActive(false);
        songIndex = rnd.Range(0, 65);
        karaokeString = data.songStrings[songIndex];
        expectedString = data.songNames[songIndex];
        Debug.LogFormat("[Broken Karaoke #{0}] The song is {1}.", moduleId, data.songNamesLogging[songIndex]);
    }
    void pressBuzzer() {
        buzzer.AddInteractionPunch(.5f);
        switch (buzzerState)
        {
           case 0:
           {   
                    StartCoroutine(playKaraoke());
                    buzzerState++;
                    break;
           }
          case 1:     
         {
                StopAllCoroutines();
                karakoe.SetActive(false);
                keyboard.SetActive(true);
                buzzerState++;
                break;
          }
          case 2:
          {
            Debug.LogFormat("[Broken Karaoke #{0}] You submitted {1}.", moduleId, inputtedString);   
            if (inputtedString == expectedString)
                    {
                        module.HandlePass();
                        Debug.LogFormat("[Broken Karaoke #{0}] That was correct. Module solved.", moduleId);
                        sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                        keyboard.SetActive(false);
                        solveText.text = data.songNamesLogging[songIndex];
                    }
            else
                    {
                        module.HandleStrike();
                        keyboard.SetActive(false);
                        karakoe.SetActive(true);
                        foreach (TextMesh text in karakoeTexts)
                        {
                            text.text = "";
                        }
                        buzzerState = 0;
                        Start();
                    }
            break;
          }
        }
	}
    void pressKey(KMSelectable key)
    {
        key.AddInteractionPunch(.5f);
        inputtedString += key.GetComponentInChildren<TextMesh>().text;
    }
    IEnumerator playKaraoke()
    {
        int i = 0;
        while (i < karakoeTexts.Length)
        {
            karakoeTexts[i].text = karaokeString[i].ToString();
            yield return new WaitForSeconds(3f);
            i++;
        }
        yield return new WaitForSeconds(3f);
        module.HandleStrike();
        Start();
        buzzerState = 0;
        foreach (TextMesh text in karakoeTexts)
        {
            text.text = "";
        }
        yield break;
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} start' to start the karaoke.  Use '!{0} stop' to stop it. use '!{0} sumbit SONGGAME' to submit e.g. '!{0} sumbit ALITTLELESSCONVERSATION";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] commandArray = command.Split(' ');
        if ((command == "start" && buzzerState == 0) || (command == "stop" && buzzerState == 1))
        {
            yield return null;
            buzzer.OnInteract();
        }
        else if (commandArray.Length == 2 && commandArray[0] == "submit" && buzzerState == 2)
        {
            foreach (char letter in commandArray[1])
            {
                foreach (KMSelectable key in keys)
                {
                    if (key.GetComponentInChildren<TextMesh>().text == letter.ToString().ToUpperInvariant())
                    {
                        yield return null;
                        key.OnInteract();
                    }
                }
            }
            buzzer.OnInteract();
        }
        else
        {
            yield return "sendtochaterror @{0}, invalid command.";
            yield break;
        }
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        while (buzzerState != 2)
        {
            yield return null;
            buzzer.OnInteract();
        }
        foreach (char letter in expectedString)
        {
            foreach (KMSelectable key in keys)
            {
                if (key.GetComponentInChildren<TextMesh>().text == letter.ToString().ToUpperInvariant())
                {
                    yield return null;
                    key.OnInteract();
                }
            }
        }
        buzzer.OnInteract();
    }
}