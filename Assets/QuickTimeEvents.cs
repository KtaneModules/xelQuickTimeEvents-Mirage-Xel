using System;
using System.Collections;
using System.Collections.Generic;
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
        new int[] {0, 3, 2, 1, 0, 0, 3, 1, 2, 1},
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
    bool TPModeEnabled;
    string[] buttonNames = new string[] { "Triangle", "Circle", "Square", "X" };
    int moduleID;
    static int moduleIDCounter;
    bool solved;

    void Awake()
    {
        moduleID = moduleIDCounter++;
        foreach (KMSelectable i in buttons)
        {
            KMSelectable pressedButton = i;
            pressedButton.OnInteract += delegate {pressButton(pressedButton, Array.IndexOf(buttons, pressedButton)); return false; };
        }
    }
    void Start () {
		foreach(GameObject i in lights)
        {
            i.SetActive(false);
        }
        curRowIndex = bomb.GetSerialNumberNumbers().First();
        curColumnIndex = bomb.GetSerialNumberNumbers().Last();
        stageCounter = 0;
        sprite.sprite = null;
    }
	
	
	void pressButton (KMSelectable button, int index) {
        if (!solved)
        {
            Debug.LogFormat("[Quick Time Events #{0}] You pressed the {1} button.", moduleID, buttonNames[index]);
            if (stageCounter == 0)
            {
                StartCoroutine(stage());
                stageCounter++;
            }
            else if (expectedButton == index)
            {
                StopAllCoroutines();
                Debug.LogFormat("[Quick Time Events #{0}] That was correct.", moduleID);
                stageCounter++;
                if (stageCounter == 6)
                {
                    Start();
                    module.HandlePass();
                    solved = true;
                    Debug.LogFormat("[Quick Time Events #{0}] Module solved.", moduleID);
                }
                StartCoroutine(stage());
            }
            else
            {
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
        Debug.LogFormat("[Quick Time Events #{0}] The button on the display is {1}.", moduleID, buttonNames[displayIndex - 1]);
        Debug.LogFormat("[Quick Time Events #{0}] The lit button is {1}.", moduleID, buttonNames[lightIndex - 1]);
        curRowIndex += displayIndex;
        curColumnIndex += lightIndex;
        expectedButton = table[curRowIndex % 10][curColumnIndex % 10];
        Debug.LogFormat("[Quick Time Events #{0}] The correct button is {1}.", moduleID, buttonNames[expectedButton]);
        if (TPModeEnabled)
        {
            yield return new WaitForSeconds(15f);
        }
        yield return new WaitForSeconds(15f);
        Debug.LogFormat("[Quick Time Events #{0}] You ran out of time! Strike!", moduleID);
        module.HandleStrike();
        Start();
        yield break;
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} triangle/square/circle/x' to press the corresponding button.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        TPModeEnabled = true;
        command = command.ToLowerInvariant();
        string[] validcmds = new string[] { "triangle", "circle", "square", "x" };
        {
                if (!validcmds.Contains(command))
                {
                    yield return "sendtochaterror Invalid command.";
                    yield break;
                }
                for (int j = 0; j < 4; j++)
                {
                    if (command == validcmds[j])
                    {
                        yield return null;
                        buttons[j].OnInteract();
                    }
                }
            }
            yield break;
        }
    }
