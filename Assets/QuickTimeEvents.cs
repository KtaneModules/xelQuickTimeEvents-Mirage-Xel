using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using KModkit;
using rnd = UnityEngine.Random;
public class QuickTimeEvents : MonoBehaviour {
    public KMSelectable[] buttons;
    public GameObject[] lights;
    public SpriteRenderer sprite;
    public Sprite[] sprites;
    int[][] table = new int[][] {
        new int[] {0, 0, 2, 3, 1, 3, 1, 2, 0, 1},
        new int[] {1, 2, 2, 3, 1, 2, 0, 2, 0, 0},
        new int[] {3, 0, 0, 1, 2, 1, 1, 0, 1, 3},
        new int[] {2, 1, 3, 0, 1, 1, 3, 1, 0, 3},
        new int[] {3, 1, 0, 0, 2, 1, 1, 3, 2, 0},
        new int[] {2, 2, 1, 1, 0, 1, 3, 0, 3, 1},
        new int[] {0, 1, 2, 3, 3, 1, 2, 0, 3, 2},
        new int[] {0, 3, 2, 1, 0, 3, 1, 2, 1, 0},
        new int[] {3, 1, 2, 2, 1, 0, 3, 2, 0, 0},
        new int[] {1, 0, 3, 1, 2, 2, 1, 0, 1, 3}
    };
    int displayIndex;
    int lightIndex;
    int curRowIndex;
    int curColumnIndex;
    int expectedButton;
    int stageCounter;
    public KMBombModule module;
    public KMBombInfo bomb;
    bool TwitchPlaysActive;
    string[] buttonNames = new string[] { "Triangle", "Square", "Circle", "X" };
    int moduleID;
    static int moduleIDCounter = 1;
    bool solved;

    void Awake()
    {
        moduleID = moduleIDCounter++;
        foreach (KMSelectable i in buttons)
        {
            KMSelectable pressedButton = i;
            pressedButton.OnInteract += delegate { pressButton(pressedButton, Array.IndexOf(buttons, pressedButton)); return false; };
        }
    }
    void Start () {
        curRowIndex = bomb.GetSerialNumberNumbers().First();
        curColumnIndex = bomb.GetSerialNumberNumbers().Last();
        if (stageCounter == 0)
            Debug.LogFormat("[Quick Time Events #{0}] The starting position is row {1}, column {2}.", moduleID, curRowIndex, curColumnIndex);
        foreach (GameObject i in lights)
        {
            i.SetActive(false);
        }
        stageCounter = 0;
        sprite.sprite = null;
    }
	
	void pressButton (KMSelectable button, int index) {
        if (!solved)
        {
            button.AddInteractionPunch(0.5f);
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
            if (stageCounter == 0)
            {
                StartCoroutine(stage());
                stageCounter++;
            }
            else if (expectedButton == index)
            {
                Debug.LogFormat("[Quick Time Events #{0}] You pressed the {1} button.", moduleID, buttonNames[index]);
                Debug.LogFormat("[Quick Time Events #{0}] That was correct.", moduleID);
                StopAllCoroutines();
                stageCounter++;
                if (stageCounter == 6)
                {
                    Start();
                    module.HandlePass();
                    solved = true;
                    Debug.LogFormat("[Quick Time Events #{0}] Module solved.", moduleID);
                    return;
                }
                StartCoroutine(stage());
            }
            else
            {
                Debug.LogFormat("[Quick Time Events #{0}] You pressed the {1} button.", moduleID, buttonNames[index]);
                Debug.LogFormat("[Quick Time Events #{0}] That was incorrect. Strike!", moduleID);
                StopAllCoroutines();
                module.HandleStrike();
                Start();
            }
        }
	}

    IEnumerator stage()
    {
        foreach (GameObject i in lights)
        {
            i.SetActive(false);
        }
        displayIndex = rnd.Range(1, 5);
        lightIndex = rnd.Range(1, 5);
        sprite.sprite = sprites[displayIndex - 1];
        lights[lightIndex - 1].SetActive(true);
        Debug.LogFormat("[Quick Time Events #{0}] The button on the display is {1}. Moving down {2}.", moduleID, buttonNames[displayIndex - 1], displayIndex);
        Debug.LogFormat("[Quick Time Events #{0}] The lit button is {1}. Moving left {2}.", moduleID, buttonNames[lightIndex - 1], lightIndex);
        curRowIndex += displayIndex;
        curColumnIndex -= lightIndex;
        expectedButton = table[mod(curRowIndex, 10)][mod(curColumnIndex, 10)];
        Debug.LogFormat("[Quick Time Events #{0}] The correct button is {1}.", moduleID, buttonNames[expectedButton]);
        if (TwitchPlaysActive)
        {
            yield return new WaitForSeconds(8f);
        }
        yield return new WaitForSeconds(7f);
        Debug.LogFormat("[Quick Time Events #{0}] You ran out of time! Strike!", moduleID);
        module.HandleStrike();
        Start();
        yield break;
    }

    int mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    #pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} t(riangle)/s(quare)/c(ircle)/x' to press the corresponding button. On Twitch Plays the timer is increased to 15 seconds.";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] validcmds = new string[] { "triangle", "square", "circle", "x", "t", "s", "c" };
        if (!validcmds.Contains(command))
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        yield return null;
        for (int j = 0; j < 7; j++)
        {
            if (command == validcmds[j])
            {
                buttons[j % 4].OnInteract();
                yield break;
            }
        }
        yield break;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (stageCounter == 0)
        {
            buttons[rnd.Range(0, 4)].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        int start = stageCounter;
        for (int i = start; i < 6; i++)
        {
            buttons[expectedButton].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}