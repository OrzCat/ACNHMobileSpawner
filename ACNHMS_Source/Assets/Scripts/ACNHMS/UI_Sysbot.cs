using UnityEngine;
using UnityEngine.UI;

public class UI_Sysbot : MonoBehaviour
{
	public const string IPKEY = "IP_SYS";
	public const string SOCKETKEY = "SOCK_SYS";

    public static UI_Sysbot LastLoadedUI_Sysbot;

    [HideInInspector]
    InjectionProtocol CurrentInjP = InjectionProtocol.Sysbot;

    public string DefaultIp = "192.168.0.1";
	public string DefaultSocket = "6000";

	public Text ConnectedText;

	public InputField IP;
	public InputField Socket;
	public GameObject RootConnected;
	public GameObject RootNotConnected;
    public GameObject RootNotConnectedAndroid;

    public GameObject RootUSB;
    public GameObject RootSYS;

    private SysBotController sysBot;
#if PLATFORM_ANDROID
    private USBBotAndroidController usbaBot;
#endif

    private void Start()
	{
		SetConnected(val: false);
		DefaultIp = PlayerPrefs.GetString("IP_SYS", DefaultIp);
		DefaultSocket = PlayerPrefs.GetString("SOCK_SYS", DefaultSocket);
		IP.text=(DefaultIp);
		Socket.text=(DefaultSocket);

        SetInjectionProtocol(UI_Settings.GetInjectionProtocol());

        LastLoadedUI_Sysbot = this;
	}

    public void SetInjectionProtocol(InjectionProtocol injp)
    {
        CurrentInjP = injp;

        RootUSB.SetActive(false);
        RootSYS.SetActive(false);

        switch (CurrentInjP)
        {
            case InjectionProtocol.Sysbot:
                RootSYS.SetActive(true);
                break;
            case InjectionProtocol.UsbBotAndroid:
                RootUSB.SetActive(true);
                break;
            default:
                RootSYS.SetActive(true);
                break;
        }

    }

	private void Update()
	{
	}

	public void AssignSysbot(SysBotController sb)
	{
		sysBot = sb;
	}
#if PLATFORM_ANDROID
    public void AssignUSBBotAndroid(USBBotAndroidController usbab)
    {
        usbaBot = usbab;
    }
#endif

    public void SetConnected(bool val)
	{
		RootConnected.SetActive(val);
		RootNotConnected.SetActive(!val);
	}

	public void TryConnect()
	{
		string error = "";
		if (!sysBot.Connect(IP.text, Socket.text, out error))
		{
			SetConnected(val: false);
			ConnectedText.text=("Connection failed: " + error);
			return;
		}
		SetConnected(true);
		ConnectedText.text=("Connected successfully");
		PlayerPrefs.SetString("IP_SYS", IP.text);
		PlayerPrefs.SetString("SOCK_SYS", Socket.text);
	}

    public void TryConnectUSBa()
    {
#if PLATFORM_ANDROID
        if (!usbaBot.Connect())
        {
            ConnectedText.text = "USB connection failed: Check popup";
            return;
        }
        RootNotConnectedAndroid.SetActive(false);
        ConnectedText.text = "USB connected";
#endif
    }

    private void OnApplicationQuit()
    {
        if (sysBot == null)
            return;
        if (sysBot.Bot.Connected)
            sysBot.Bot.Disconnect();
    }
}
