using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using SimpleJSON;

public struct PlanetPacket {

    string date;
    Vector3 coordinates;

    public PlanetPacket(string _date, Vector3 _coordinates) {
        date = _date;
        coordinates = _coordinates;
    }

    public string GetDate() {
        return date;
    }

    public Vector3 GetCoordinates() {
        return coordinates;
    }

}

public class WebSocketReceiver : MonoBehaviour {

    public string serverAddress = "localhost";
    public int serverPort = 5555;
    Client ws;

    Queue<PlanetPacket[]> messages = new Queue<PlanetPacket[]>();


    public delegate void PacketReceiveAction(PlanetPacket p0, PlanetPacket p1);
    public static event PacketReceiveAction OnReceive;

    void Start () {
        ws = new Client("http://" + serverAddress + ":" + serverPort);
        Debug.Log("Settings up websocket");
        ws.Message += (sender, e) =>
        {
            Debug.Log("Received message " + e.Message.MessageText);
            JSONNode json = JSON.Parse(e.Message.MessageText);
            JSONNode points = json["points"];
            float p0x = points[0]["co"][0].AsFloat;
            float p0y = points[0]["co"][1].AsFloat;
            float p0z = points[0]["co"][2].AsFloat;
            float p1x = points[1]["co"][0].AsFloat;
            float p1y = points[1]["co"][1].AsFloat;
            float p1z = points[1]["co"][2].AsFloat;
            PlanetPacket p0 = new PlanetPacket("", new Vector3(p0x, p0y, p0z));
            PlanetPacket p1 = new PlanetPacket("", new Vector3(p1x, p1y, p1z));
            messages.Enqueue(new PlanetPacket[] { p0, p1 });
        };
        ws.Error += (sender, e) =>
        {
            Debug.Log("Error: " + e.Message);
        };
        //ws.Send("BALUS");
        ws.Connect();
    }

	void Update () {
		while(messages.Count > 0)
        {
            PlanetPacket[] coords = messages.Dequeue();
            if(OnReceive != null)
            {
                OnReceive(coords[0], coords[1]);
            }
        }
	}
}
