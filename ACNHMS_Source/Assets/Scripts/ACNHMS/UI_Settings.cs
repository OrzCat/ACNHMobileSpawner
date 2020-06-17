﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using NHSE.Core;

public enum StringSearchMode 
{
    Contains = 0,
    StartsWith = 1
}

public enum InjectionProtocol
{
    Sysbot = 0,
    UsbBotAndroid = 1
}

public class UI_Settings : MonoBehaviour
{
    public const string SEARCHMODEKEY = "SMODEKEY";
    public const string ITEMLANGMODEKEY = "ITEMLMKEY";
    public const string INJMODEKEY = "INJKEY";

    public Dropdown LanguageField;
    public Dropdown SearchMode;
    public Dropdown InjectionMode;
    public InputField Offset;

    // Start is called before the first frame update
    void Start()
    {
        SearchMode.ClearOptions();
        string[] smChoices = Enum.GetNames(typeof(StringSearchMode));
        foreach(string sm in smChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = sm;
            SearchMode.options.Add(newVal);
        }
        SearchMode.value = (int)GetSearchMode();
        SearchMode.RefreshShownValue();

        LanguageField.ClearOptions();
        string[] langChoices = GameLanguage.AvailableLanguageCodes;
        foreach(string lm in langChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = lm;
            LanguageField.options.Add(newVal);
        }
        LanguageField.value = GetLanguage();
        LanguageField.RefreshShownValue();

        Offset.text = SysBotController.CurrentOffset;

#if PLATFORM_ANDROID
        InjectionMode.ClearOptions();
        string[] injChoices = Enum.GetNames(typeof(InjectionProtocol));
        foreach (string insj in injChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = insj;
            InjectionMode.options.Add(newVal);
        }
        InjectionMode.value = (int)GetInjectionProtocol();
        InjectionMode.RefreshShownValue();
        InjectionMode.onValueChanged.AddListener(delegate {
            SetInjectionProtocol((InjectionProtocol)InjectionMode.value);
            if (UI_Sysbot.LastLoadedUI_Sysbot != null)
                UI_Sysbot.LastLoadedUI_Sysbot.SetInjectionProtocol((InjectionProtocol)InjectionMode.value);
        });
#else
        InjectionMode.gameObject.SetActive(false);
#endif

        SearchMode.onValueChanged.AddListener(delegate { setSearchMode((StringSearchMode)SearchMode.value); });
        LanguageField.onValueChanged.AddListener(delegate { SetLanguage(LanguageField.value); });
        Offset.onValueChanged.AddListener(delegate { SysBotController.CurrentOffset = Offset.text; });

        SetLanguage(GetLanguage());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static InjectionProtocol GetInjectionProtocol(InjectionProtocol imDefault = InjectionProtocol.Sysbot)
    {
        return (InjectionProtocol)PlayerPrefs.GetInt(INJMODEKEY, (int)imDefault);
    }

    public static void SetInjectionProtocol(InjectionProtocol injp)
    {
        PlayerPrefs.SetInt(INJMODEKEY, (int)injp);
    }

    public static StringSearchMode GetSearchMode(StringSearchMode ssmDefault = StringSearchMode.Contains)
    {
        return (StringSearchMode)PlayerPrefs.GetInt(SEARCHMODEKEY, (int)ssmDefault);
    }

    private static void setSearchMode(StringSearchMode ssm)
    {
        PlayerPrefs.SetInt(SEARCHMODEKEY, (int)ssm);
    }

    public static int GetLanguage(int defLang = 0)
    {
        return PlayerPrefs.GetInt(ITEMLANGMODEKEY, defLang);
    }

    public static void SetLanguage(int nLang)
    {
        PlayerPrefs.SetInt(ITEMLANGMODEKEY, nLang);
        GameInfo.SetLanguage2Char(nLang);
    }
}
