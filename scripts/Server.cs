using Godot;
using System;
using System.Collections.Generic;

public class Server : Node {

    public const int PORT = 12364;
    public const int MAX_PLAYERS = 20;
    public Dictionary<int, string> players = new Dictionary<int, string>();
    string playerName;

    public override void _Ready() {
        var host = new NetworkedMultiplayerENet();
        var err = host.CreateServer(PORT, 20);

        if (err != Error.Ok) {
            JoinServer();
            return;
        }

        GetTree().SetNetworkPeer(host);
        SpawnPlayer(1);
    }

    public override void _Process(float delta) {

    }

    void JoinServer() {
        playerName = "Client";
        var host = new NetworkedMultiplayerENet();
        host.CreateClient("127.0.0.1", PORT);
        GetTree().SetNetworkPeer(host);
    }

    private void OnPlayerConnected(int id) {

    }

    private void OnPlayerDisconnected(int id) {
        UnregisterPlayer(id);
        Rpc("unregister_player", id); //Remote func
    }

    void UnregisterPlayer(int id) {
        GetNode("/root/" + id).QueueFree();
        players.Remove(id);
    }

    private void ConnectedOk() {
        RpcId(1, "user_ready", GetTree().GetNetworkUniqueId(), playerName);
    }


    void user_ready(int id, string player_name) {
        if (GetTree().IsNetworkServer())
            RpcId(id, "register_in_game");
    }


    void register_in_game() {
        Rpc("register_new_player", GetTree().GetNetworkUniqueId(), playerName);
        register_new_player(GetTree().GetNetworkUniqueId(), playerName);
    }

    private void onServerDisconnected() {
        QuitGame();
    }


    void register_new_player(int id, string name) {
        if (GetTree().IsNetworkServer()) {
            RpcId(id, "register_new_player", 1, playerName);
            foreach (int peer_id in players.Keys)
                RpcId(id, "register_new_player", peer_id, players[peer_id]);
        }
        players[id] = name;
        SpawnPlayer(id);
    }


    void register_player(int id, string name) {
        if (GetTree().IsNetworkServer()) {
            RpcId(id, "register_player", 1, playerName);
            foreach (int peer_id in players.Keys) {
                RpcId(id, "register_player", peer_id, players[peer_id]);
                RpcId(peer_id, "register_player", id, name);
            }
        }
        players[id] = name;
    }

    void unregister_player(int id) {
        GetNode("/root/" + id).QueueFree();
        players.Remove(id);
    }

    public void QuitGame() {
        GetTree().SetNetworkPeer(null);
        players.Clear();
    }

    void SpawnPlayer(int id) {
        var player_scene = ResourceLoader.Load("res://scenes/Chunk.tscn") as PackedScene;
        var player = player_scene.Instance();
        player.SetName(""+id);
        if (id == GetTree().GetNetworkUniqueId()) {
            player.SetNetworkMaster(id);
            //player.player_id = id;
            //player.control = true;
        }
        GetParent().AddChild(player);
    }
}
