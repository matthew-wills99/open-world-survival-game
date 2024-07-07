using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static Utils;
using ExtrasClipperLib;
using System;

public class MenuManager : MonoBehaviour
{
    public MapManager mapManager;

    public GameObject menuCanvas;

    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject playMenu;
    public GameObject quitMenu;
    public GameObject newGameMenu;

    public GameObject cursor;

    Vector3 selMenuPos;
    float prefWidth;
    public int cursorPlacementBuffer = 40;

    // main menu buttons
    public TextMeshProUGUI playText;
    public TextMeshProUGUI optionsText;
    public TextMeshProUGUI quitText;

    // play menu buttons
    public TextMeshProUGUI newGameText;
    public TextMeshProUGUI loadGameText;
    public TextMeshProUGUI playBackText;

    // options menu buttons
    public TextMeshProUGUI optionsBackText;

    // quit menu buttons
    public TextMeshProUGUI quitYesText;
    public TextMeshProUGUI quitNoText;

    // new game menu buttons
    public TextMeshProUGUI worldNameText;
    public TextMeshProUGUI worldSizeText;
    public TextMeshProUGUI seedText;
    public TextMeshProUGUI createWorldText;
    public TextMeshProUGUI newGameBackText;

    public TMP_InputField worldNameInput;
    public TextMeshProUGUI worldSizeSelection;
    public TMP_InputField seedInput;
    
    Dictionary<MenuState, GameObject> menuPages;

    List<TextMeshProUGUI> mainMenuSelections;
    List<TextMeshProUGUI> playMenuSelections;
    List<TextMeshProUGUI> optionsMenuSelections;
    List<TextMeshProUGUI> quitMenuSelections;
    List<TextMeshProUGUI> newGameMenuSelections;

    List<TextMeshProUGUI> selectedMenuSelections;

    int cursorPos;

    MenuState menuState;

    bool typingSeed;
    bool typingWorldName;

    MapSize worldSize;
    int worldSizeIndex;
    List<MapSize> worldSizes;

    void SetMenuState(MenuState state)
    {
        foreach(MenuState page in menuPages.Keys)
        {
            if(page != state)
            {
                menuPages[page].gameObject.SetActive(false);
            }
            else
            {
                menuPages[page].gameObject.SetActive(true);
            }
        }
        menuState = state;
    }

    void SetCursor(int newPos)
    {
        cursorPos = newPos;
        prefWidth = selectedMenuSelections[cursorPos].GetPreferredValues().x;
        selMenuPos = selectedMenuSelections[cursorPos].transform.position;
        cursor.transform.position = new Vector3(selMenuPos.x - (prefWidth / 2) - cursorPlacementBuffer, selMenuPos.y, 0);
    }

    void MoveCursor(int dir)
    {
        switch(menuState)
        {
            case MenuState.Main:
                selectedMenuSelections = mainMenuSelections;
                break;
            case MenuState.Play:
                selectedMenuSelections = playMenuSelections;
                break;
            case MenuState.Options:
                selectedMenuSelections = optionsMenuSelections;
                break;
            case MenuState.Quit:
                selectedMenuSelections = quitMenuSelections;
                break;
            case MenuState.NewGame:
                selectedMenuSelections = newGameMenuSelections;
                break;
            case MenuState.LoadGame:
                break;
        }
        if(cursorPos + dir >= selectedMenuSelections.Count)
        {
            cursorPos = 0;
        }
        else if(cursorPos + dir < 0)
        {
            cursorPos = selectedMenuSelections.Count - 1;
        }
        else
        {
            cursorPos += dir;
        }
        prefWidth = selectedMenuSelections[cursorPos].GetPreferredValues().x;
        selMenuPos = selectedMenuSelections[cursorPos].transform.position;
        cursor.transform.position = new Vector3(selMenuPos.x - (prefWidth / 2) - cursorPlacementBuffer, selMenuPos.y, 0);
    }

    void UpdateMenu(string selection)
    {
        switch(menuState)
        {
            case MenuState.Main:
                switch(selection)
                {
                    case "PLAY":
                        SetMenuState(MenuState.Play);
                        SetCursor(0);
                        MoveCursor(0);
                        break;
                    case "OPTIONS":
                        SetMenuState(MenuState.Options);
                        SetCursor(0);
                        MoveCursor(0);
                        break;
                    case "QUIT":
                        SetMenuState(MenuState.Quit);
                        SetCursor(0);
                        MoveCursor(0);
                        break;
                }
                break;
            case MenuState.Play:
                switch(selection)
                {
                    case "NEW GAME":
                        SetMenuState(MenuState.NewGame);
                        SetCursor(0);
                        MoveCursor(0);
                        break;
                    case "LOAD GAME":
                        Debug.Log("load game");
                        break;
                    case "BACK":
                        SetMenuState(MenuState.Main);
                        SetCursor(0);
                        MoveCursor(0);
                        break;
                }
                break;
            case MenuState.Options:
                switch(selection)
                {
                    case "BACK":
                        SetMenuState(MenuState.Main);
                        SetCursor(0);
                        MoveCursor(0);
                        break;
                }
                break;
            case MenuState.Quit:
                switch(selection)
                {
                    case "YES":
                        Quit();
                        break;
                    case "NO":
                        SetMenuState(MenuState.Main);
                        SetCursor(0);
                        MoveCursor(0);
                        break;
                }
                break;
            case MenuState.NewGame:
                switch(selection)
                {
                    case "WORLD NAME:":
                        EnterWorldName();
                        break;
                    case "WORLD SIZE:":
                        CycleWorldSize();
                        break;
                    case "SEED:":
                        EnterSeed();
                        break;
                    case "CREATE WORLD":
                        GenerateWorld();
                        break;
                    case "BACK":
                        SetMenuState(MenuState.Play);
                        SetCursor(0);
                        MoveCursor(0);
                        break;
                }
                break;
        }
    }

    void Quit()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }

    void CycleWorldSize()
    {
        if(worldSizeIndex + 1 >= worldSizes.Count)
        {
            worldSizeIndex = 0;
        }
        else
        {
            worldSizeIndex += 1;
        }
        
        worldSize = worldSizes[worldSizeIndex];

        worldSizeSelection.text = worldSize.ToString();
    }

    void EnterSeed()
    {
        if(typingSeed)
        {
            typingSeed = false;
            seedInput.interactable = false;
            seedInput.DeactivateInputField();
            return;
        }
        seedInput.interactable = true;
        typingSeed = true;
        seedInput.Select();
        seedInput.ActivateInputField();
    }

    void EnterWorldName()
    {
        if(typingWorldName)
        {
            typingWorldName = false;
            worldNameInput.interactable = false;
            worldNameInput.DeactivateInputField();
            return;
        }
        worldNameInput.interactable = true;
        typingWorldName = true;
        worldNameInput.Select();
        worldNameInput.ActivateInputField();
    }

    void GenerateWorld()
    {
        Debug.Log($"Generating {worldNameInput.text}, a {worldSize} world with seed {seedInput.text}");
        
        mapManager.Startup(worldSize, int.Parse(seedInput.text));

        menuCanvas.SetActive(false);
    }

    void Update()
    {
        if(!typingSeed && !typingWorldName)
        {
            // up
            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveCursor(-1); // moving up is like going backwards
            }
            // down
            if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveCursor(1);
            }
        }
        // enter
        if(Input.GetKeyDown(KeyCode.Return))
        {
            UpdateMenu(selectedMenuSelections[cursorPos].text);
        }
        // escape
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(typingSeed || typingWorldName)
            {
                UpdateMenu(selectedMenuSelections[cursorPos].text);
            }
            else
            {
                switch(menuState)
                {
                case MenuState.Main:
                    SetMenuState(MenuState.Quit);
                    SetCursor(0);
                    MoveCursor(0);
                    break;
                case MenuState.Play:
                case MenuState.Options:
                case MenuState.Quit:
                    SetMenuState(MenuState.Main);
                    SetCursor(0);
                    MoveCursor(0);
                    break;
                case MenuState.NewGame:
                case MenuState.LoadGame:
                    SetMenuState(MenuState.Play);
                    SetCursor(0);
                    MoveCursor(0);
                    break;
                }
            }
        }
    }

    void Start()
    {   
        menuPages = new Dictionary<MenuState, GameObject>
        {
            {MenuState.Main, mainMenu},
            {MenuState.Play, playMenu},
            {MenuState.Options, optionsMenu},
            {MenuState.Quit, quitMenu},
            {MenuState.NewGame, newGameMenu}
        };

        mainMenuSelections = new List<TextMeshProUGUI>
        {
            playText,
            optionsText,
            quitText
        };

        playMenuSelections = new List<TextMeshProUGUI>
        {
            newGameText,
            loadGameText,
            playBackText
        };

        optionsMenuSelections = new List<TextMeshProUGUI>
        {
            optionsBackText
        };

        quitMenuSelections = new List<TextMeshProUGUI>
        {
            quitYesText,
            quitNoText
        };

        newGameMenuSelections = new List<TextMeshProUGUI>
        {
            worldNameText,
            worldSizeText,
            seedText,
            createWorldText,
            newGameBackText
        };

        // initialize cursor position to first in list
        cursorPos = 0;
        MoveCursor(0);
        SetCursor(0);

        SetMenuState(MenuState.Main);

        typingSeed = false;
        typingWorldName = false;

        seedInput.interactable = false;
        worldNameInput.interactable = false;

        worldSizes = new List<MapSize>
        {
            MapSize.Small,
            MapSize.Medium,
            MapSize.Large
        };
        worldSizeIndex = worldSizes.IndexOf(MapSize.Small);
        worldSize = MapSize.Small;
        worldSizeSelection.text = worldSize.ToString();
    }
}
