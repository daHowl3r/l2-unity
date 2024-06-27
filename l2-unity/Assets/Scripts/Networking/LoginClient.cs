using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

public class LoginClient : DefaultClient {
    private LoginClientPacketHandler clientPacketHandler;
    private LoginServerPacketHandler serverPacketHandler;

    public LoginClientPacketHandler ClientPacketHandler { get { return clientPacketHandler; } }
    public LoginServerPacketHandler ServerPacketHandler { get { return serverPacketHandler; } }


    private static LoginClient _instance;
    public static LoginClient Instance { get { return _instance; } }

    private void Awake() {
        if (_instance == null) {
            _instance = this;
        } else if (_instance != this) {
            Destroy(this);
        }
    }

    protected override void CreateAsyncClient() {
        clientPacketHandler = new LoginClientPacketHandler();
        serverPacketHandler = new LoginServerPacketHandler();

        _client = new AsynchronousClient(_serverIp, _serverPort, this, clientPacketHandler, serverPacketHandler);
    }

    protected override void OnConnectionSuccess() {
        
    }

    public override void OnConnectionFailed() {
        base.OnConnectionFailed();
    }

    public override void OnAuthAllowed() {
        base.OnAuthAllowed();
    }

    public override void OnDisconnect() {
        base.OnDisconnect();
    }
}
