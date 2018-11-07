using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerControll : NetworkBehaviour
{
    public float velocidade;
    private GameObject player;
    private bool fimDeJogo;

    private GameObject textFimDeJogo;
    private GameObject painelFimDeJogo;

    void Start()
    {
        fimDeJogo = false;
        player = GameObject.FindWithTag("Torta");
        textFimDeJogo = GameObject.FindWithTag("FimDaPartidaText");
        painelFimDeJogo = GameObject.FindWithTag("FimDaPartidaPainel");

        if (!isLocalPlayer)
            return;

        painelFimDeJogo.gameObject.SetActive(false);

        Debug.Log(GetNetworkChannel());
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase.Equals(TouchPhase.Began) && !fimDeJogo)
            {
                if (netId.Value == 1)
                {
                    CmdFire(velocidade);
                    
                }
                else
                {
                    CmdFire(-velocidade);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(netId.Value);
            if (netId.Value == 1)
            {
                CmdFire(velocidade);
            }
            else
            {
                CmdFire(-velocidade);
            }
        }

        if (!fimDeJogo)
        {
            VerificarFimDeJogo();
        }
    }

    // This [Command] code is called on the Client …
    // … but it is run on the Server!
    [Command]
    void CmdFire(float ve)
    {
        if (!isServer)
            return;

        //Debug.Log("TesteFire-Server");
        RpcFire(ve);
    }

    //executado em cada cliente
    [ClientRpc]
    void RpcFire(float v)
    {
        Debug.Log("cliente teste rpc "+netId.Value+" = " + v);

        player.transform.position += new Vector3(v, 0, 0);
    }

    void VerificarFimDeJogo()
    {
        if (player.transform.localPosition.x >= 1)
        {
            if (netId.Value == 1)
            {
                Ganhador(netId.Value);
            }
            else
            {
                Perdedor(netId.Value);
            }
        }
        else
        {
            if (player.transform.localPosition.x <= -1)
            {
                if (netId.Value == 1)
                {
                    Perdedor(netId.Value);
                }
                else
                {
                    Ganhador(netId.Value);
                }
            }
        }
    }

    void Ganhador(uint id)
    {
        Debug.Log(id + " Eu sou o ganhador!!!");
        fimDeJogo = true;

        painelFimDeJogo.gameObject.SetActive(true);
        textFimDeJogo.GetComponent<Text>().text = "Voce Ganhou Parabens!!";
    }

    void Perdedor(uint id)
    {
        Debug.Log(id + " Eu sou o perdedor!!!");
        fimDeJogo = true;

        painelFimDeJogo.gameObject.SetActive(true);
        textFimDeJogo.GetComponent<Text>().text = "Voce Perdeu Tente Novamente!!";
    }

    public void ReiniciarPartida()
    {
        Debug.Log("Reiniciou a partida jogador " + netId.Value);
    }

    public void SairDaPartida()
    {
        Debug.Log("Sair da Partida jogador " + netId.Value);
    }

}