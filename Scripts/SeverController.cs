using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class SeverController : NetworkManager
{

    //referencias dos paineis inicio
    public Image PainelInicio;
    public InputField ipDoServidor;
    public Button conectar;
    public Button criarServidor;

    //referencias dos paineis criarServidor
    public Image PainelCriarServidor;
    public InputField seuIP;
    public Button cancelarCriarServidor;

    //painel do vuforia
    public Image PainelScan;

    //public Image PainelMenu;
    public Image PainelServidor;
    public Image PainelCliente;

    //painel fimDaPartida
    public Image PainelFimDaPartida;

    private NetworkClient myClient;

    public Button Sair;

    private GameObject Torta;


    // Use this for initialization
    void Start()
    {
        //set para o painel criar servidor sumir
        PainelCriarServidor.gameObject.SetActive(false);
        PainelScan.gameObject.SetActive(false);
        PainelServidor.gameObject.SetActive(false);
        PainelCliente.gameObject.SetActive(false);

        Debug.Log("ok inicializou server");

        //execucao das funcoes de click dos botoes do painel inicio
        conectar.onClick.AddListener(delegate { conectarAoServer(ipDoServidor.text); });
        criarServidor.onClick.AddListener(createAServer);

        //execucao das funcoes de click dos botoes do painel criarservidor
        cancelarCriarServidor.onClick.AddListener(cancelarCriacaoDoServidor);

        //para botao sair
        Sair.onClick.AddListener(DisconectarESair);

        //get na torta
        Torta = GameObject.FindWithTag("Torta");

    }

    //funcao para se conectar em um servidor
    private void conectarAoServer(string ip)
    {
        if (ip.Equals(""))
        {
            Debug.Log("Digite ip valido Porfavor");
        }
        else
        {
            Debug.Log("conectar no servidor: " + ip + ":" + networkPort);
            networkAddress = ip;
            StartClient();
            PainelInicio.gameObject.SetActive(false);
        }

    }

    //funcao para criar um servidor
    private void createAServer()
    {
        //pega endereço ip local da minha maquina para criar um servidor
        networkAddress = GetIpv4();

        Debug.Log("criar servidor");
        Debug.Log(networkAddress);
        seuIP.text = networkAddress;
        StartHost();
        PainelInicio.gameObject.SetActive(false);
        PainelCriarServidor.gameObject.SetActive(true);
    }

    //funcao para cancelar a criacao do servidor
    private void cancelarCriacaoDoServidor()
    {
        DisconectarESair();
        StopServer();
        PainelCriarServidor.gameObject.SetActive(false);
        PainelInicio.gameObject.SetActive(true);

    }

    private void DisconectarESair()
    {
        PainelFimDaPartida.gameObject.SetActive(true);
        PainelServidor.gameObject.SetActive(false);
        PainelCliente.gameObject.SetActive(false);
        Torta.transform.position = new Vector3(0.022f, 0, 0);
        StopClient();
    }

    //callbacks do servidor
    //==================================================================================================

    //detecta o status quando o server for inicializado
    public override void OnStartServer()
    {
        //Output that the Server has started
        Debug.Log("Server Started!");
    }

    //detecta o status quando o server for parado
    public override void OnStopServer()
    {
        //Output that the Server has stopped
        Debug.Log("Server Stopped!");
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {

        Debug.Log("Server network error occurred: " + (NetworkError)errorCode);

    }

    public override void OnServerConnect(NetworkConnection conn)
    {

        Debug.Log("A client connected to the server: " + conn);

        if(conn.connectionId == 1)
        {
            PainelCriarServidor.gameObject.SetActive(false);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {

        NetworkServer.DestroyPlayersForConnection(conn);

        if (conn.lastError != NetworkError.Ok)
        {

            if (LogFilter.logError) { Debug.LogError("ServerDisconnected due to error: " + conn.lastError); }

        }

        Debug.Log("A client disconnected from the server: " + conn);

    }

    public override void OnServerReady(NetworkConnection conn)
    {

        NetworkServer.SetClientReady(conn);

        Debug.Log("Client is set to the ready state (ready to receive state updates): " + conn);

    }

    // Client callbacks
    //====================================================================================================

    public override void OnClientConnect(NetworkConnection conn)

    {

        base.OnClientConnect(conn);

        Debug.Log("Connected successfully to server, now to set up other stuff for the client...");

        if(conn.connectionId == 0)
        {
            PainelServidor.gameObject.SetActive(true);
        }
        else
        {
            PainelCliente.gameObject.SetActive(true);
        }
        
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {

        StopClient();

        if (conn.lastError != NetworkError.Ok)

        {

            if (LogFilter.logError) { Debug.LogError("ClientDisconnected due to error: " + conn.lastError); }

        }

        Debug.Log("Client disconnected from server: " + conn);

    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {

        Debug.Log("Client network error occurred: " + (NetworkError)errorCode);

    }

    public override void OnClientNotReady(NetworkConnection conn)
    {

        Debug.Log("Server has set client to be not-ready (stop getting state updates)");

    }

    public override void OnStartClient(NetworkClient client)
    {

        Debug.Log("Client has started");

    }

    public override void OnStopClient()
    {

        Debug.Log("Client has stopped");

    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {

        base.OnClientSceneChanged(conn);

        Debug.Log("Server triggered scene change and we've done the same, do any extra work here for the client...");

    }
    //=====================================================================================================
    //funcao para pegar o ip local da minha maquina retorna uma string com o ip
    public static string GetIpv4()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        NetworkInterfaceType _type = NetworkInterfaceType.Wireless80211;
        string output = "";
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        output = ip.Address.ToString();
                    }
                }
            }
        }
        return output;
#else
            return GetIpv4Mobile();
#endif


    }
    //caso for mobile ele executa essa funcao
    private static string GetIpv4Mobile()
    {
        string deviceHostName = Dns.GetHostName();
        var ipCount = Dns.GetHostAddresses(deviceHostName);
        for (int i = 0; i < ipCount.Length; i++)
        {
            IPAddress deviceIP = ipCount[i];
            if (deviceIP.AddressFamily == AddressFamily.InterNetwork)
            {
                return deviceIP.ToString();
            }
        }
        return null;
    }
}