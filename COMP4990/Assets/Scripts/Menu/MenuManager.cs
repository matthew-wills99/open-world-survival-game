using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static Utils;
using ExtrasClipperLib;
using System;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public MapManager mapManager;
    public SaveWorldScript saveWorldScript;

    public TMP_FontAsset font;
    public int fontSize = 48;

    public GameObject menuCanvas;

    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject playMenu;
    public GameObject quitMenu;
    public GameObject newGameMenu;
    public GameObject loadGameMenu;
    public GameObject confirmLoadGameMenu;

    public GameObject worldList;

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

    // load game menu buttons
    public TextMeshProUGUI worldText1;
    public TextMeshProUGUI worldText2;
    public TextMeshProUGUI worldText3;
    public TextMeshProUGUI loadGameBackText;

    public TextMeshProUGUI noWorldsText;
    int selectedWorld;

    // confirm load game menu buttons
    public TextMeshProUGUI loadYesText;
    public TextMeshProUGUI loadNoText;

    public TextMeshProUGUI confirmWorldNameText;

    
    Dictionary<MenuState, GameObject> menuPages;

    List<TextMeshProUGUI> mainMenuSelections;
    List<TextMeshProUGUI> playMenuSelections;
    List<TextMeshProUGUI> optionsMenuSelections;
    List<TextMeshProUGUI> quitMenuSelections;
    List<TextMeshProUGUI> newGameMenuSelections;
    List<TextMeshProUGUI> loadGameMenuSelections;
    List<TextMeshProUGUI> confirmLoadGameMenuSelections;

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
        SetCursor(0);
        MoveCursor(0);
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
                selectedMenuSelections = loadGameMenuSelections;
                break;
            case MenuState.ConfirmLoadGame:
                selectedMenuSelections = confirmLoadGameMenuSelections;
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

    void ToggleLoadGameScreen() // where did it go??
    {
        List<string> worlds = saveWorldScript.GetAllWorlds();
        selectedWorld = 0;
        switch(worlds.Count)
        {
            case 0:
                noWorldsText.text = "NO WORLDS";
                cursor.SetActive(false);
                return;
            case 1:
                worldText1.text = worlds[0];
                return;
            case 2:
                worldText1.text = worlds[0];
                worldText2.text = worlds[1];
                return;
            case 3:
                worldText1.text = worlds[0];
                worldText2.text = worlds[1];
                worldText3.text = worlds[2];
                return;
            default:
                worldText1.text = worlds[0];
                worldText2.text = worlds[1];
                worldText3.text = worlds[2];
                return;
        }
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
                        break;
                    case "OPTIONS":
                        SetMenuState(MenuState.Options);
                        break;
                    case "QUIT":
                        SetMenuState(MenuState.Quit);
                        break;
                }
                break;
            case MenuState.Play:
                switch(selection)
                {
                    case "NEW GAME":
                        SetMenuState(MenuState.NewGame);
                        break;
                    case "LOAD GAME":
                        ToggleLoadGameScreen();
                        SetMenuState(MenuState.LoadGame);
                        break;
                    case "BACK":
                        SetMenuState(MenuState.Main);
                        break;
                }
                break;
            case MenuState.Options:
                switch(selection)
                {
                    case "BACK":
                        SetMenuState(MenuState.Main);
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
                        break;
                }
                break;
            case MenuState.LoadGame:
                switch(selection)
                {
                    case "BACK":
                        SetMenuState(MenuState.Play);
                        break;
                    default:
                        SetMenuState(MenuState.ConfirmLoadGame);
                        confirmWorldNameText.text = saveWorldScript.GetAllWorlds()[selectedWorld];
                        break;
                }
                break;

            case MenuState.ConfirmLoadGame:
                switch(selection)
                {
                    case "YES":
                        Debug.Log("YES CONFIRM LOAD WORLD");
                        break;
                    case "NO":
                        ToggleLoadGameScreen();
                        SetMenuState(MenuState.LoadGame);
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
        
        mapManager.Startup(worldNameInput.text, worldSize, int.Parse(seedInput.text));

        menuCanvas.SetActive(false);
    }

    void CycleLoadWorlds(int p)
    {
        var allWorlds = saveWorldScript.GetAllWorlds();
        if(p == -1)
        {
            // up from 0
            if(cursorPos == 0)
            {
                MoveCursor(-1);
                return;
            }
            // 3 or less worlds to display
            if(allWorlds.Count <= 3)
            {
                // up from back button
                if(cursorPos == loadGameMenuSelections.Count - 1)
                {
                    MoveCursor(-1);
                    selectedWorld = allWorlds.Count - 1;
                    return;
                }
                MoveCursor(-1);
                selectedWorld -= 1;
                return;
            }
            // more than 3 worlds to display
            if(allWorlds.Count > 3)
            {
                // up from back button
                if(cursorPos == loadGameMenuSelections.Count - 1)
                {
                    selectedWorld = allWorlds.Count - 1;
                    worldText1.text = allWorlds[selectedWorld - 2];
                    worldText2.text = allWorlds[selectedWorld - 1];
                    worldText3.text = allWorlds[selectedWorld];
                    MoveCursor(-1);
                    return;
                }
                // 1 up neighbour left
                if(selectedWorld == 1)
                {
                    MoveCursor(-1);
                    selectedWorld -= 1;
                    return;
                }
                // up from 3rd world
                if(cursorPos == 2)
                {
                    MoveCursor(-1);
                    selectedWorld -= 1;
                    return;
                }
                // if cursor pos is in the middle
                if(cursorPos == 1)
                {
                    // if there are more than 1 up neighbours
                    if(selectedWorld > 1)
                    {
                        selectedWorld -= 1;
                        worldText1.text = allWorlds[selectedWorld - 1];
                        worldText2.text = allWorlds[selectedWorld];
                        worldText3.text = allWorlds[selectedWorld + 1];
                        return;
                    }
                    MoveCursor(-1);
                    selectedWorld -= 1;
                    return;
                }
            }

        }
        if(p == 1)
        {
            // down from last world (and not on back button)
            if(selectedWorld == allWorlds.Count - 1 && cursorPos != loadGameMenuSelections.Count - 1)
            {
                MoveCursor(1);
                return;
            }
            // 3 or less worlds to display
            if(allWorlds.Count <= 3)
            {
                // down from back button
                if(cursorPos == loadGameMenuSelections.Count - 1)
                {
                    MoveCursor(1);
                    selectedWorld = 0;
                    return;
                }
                MoveCursor(1);
                selectedWorld += 1;
                return;
            }
            // more than 3 worlds to dislay
            if(allWorlds.Count > 3)
            {
                // down from back button
                if(cursorPos == loadGameMenuSelections.Count - 1)
                {
                    selectedWorld = 0;
                    worldText1.text = allWorlds[0];
                    worldText2.text = allWorlds[1];
                    worldText3.text = allWorlds[2];
                    MoveCursor(1);
                    return;
                }
                // 1 down neighbour left
                if(selectedWorld == allWorlds.Count - 2)
                {
                    MoveCursor(1);
                    selectedWorld += 1;
                    return;
                }
                // down from 1st world
                if(cursorPos == 0)
                {
                    MoveCursor(1);
                    selectedWorld += 1;
                    return;
                }
                // if cursor pos is in the middle
                if(cursorPos == 1)
                {
                    // if there are more than 1 down neighbours
                    if(selectedWorld < allWorlds.Count - 2)
                    {
                        selectedWorld += 1;
                        worldText1.text = allWorlds[selectedWorld - 1];
                        worldText2.text = allWorlds[selectedWorld];
                        worldText3.text = allWorlds[selectedWorld + 1];
                        return;
                    }
                    MoveCursor(1);
                    selectedWorld += 1;
                    return;
                }
            }
        }
    }

    //+1 goes down 
    //-1 goes up

    void Update()
    {
        if(!typingSeed && !typingWorldName)
        {
            // up
            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                if(menuState == MenuState.LoadGame)
                {
                    CycleLoadWorlds(-1);
                }
                else
                {
                    MoveCursor(-1); // moving up is like going backwards
                }
            }
            // down
            if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if(menuState == MenuState.LoadGame)
                {
                    CycleLoadWorlds(1);
                }
                else
                {
                    MoveCursor(1); // moving up is like going backwards
                }
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
                    break;
                case MenuState.Play:
                case MenuState.Options:
                case MenuState.Quit:
                    SetMenuState(MenuState.Main);
                    break;
                case MenuState.NewGame:
                case MenuState.LoadGame:
                    SetMenuState(MenuState.Play);
                    break;
                case MenuState.ConfirmLoadGame:
                    ToggleLoadGameScreen();
                    SetMenuState(MenuState.LoadGame);
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
            {MenuState.NewGame, newGameMenu},
            {MenuState.LoadGame, loadGameMenu},
            {MenuState.ConfirmLoadGame, confirmLoadGameMenu}
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

        loadGameMenuSelections = new List<TextMeshProUGUI>();

        selectedWorld = 0;

        //worldTexts = new List<TextMeshProUGUI>();

        switch(saveWorldScript.GetAllWorlds().Count)
        {
            case 0:
                break;
            case 1:
                loadGameMenuSelections.Add(worldText1);
                break;
            case 2:
                loadGameMenuSelections.Add(worldText1);
                loadGameMenuSelections.Add(worldText2);
                break;
            default:
                loadGameMenuSelections.Add(worldText1);
                loadGameMenuSelections.Add(worldText2);
                loadGameMenuSelections.Add(worldText3);
                break;
        }

        loadGameMenuSelections.Add(loadGameBackText);

    
        confirmLoadGameMenuSelections = new List<TextMeshProUGUI>
        {
            loadYesText,
            loadNoText
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
