﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class PlayerMove : NetworkBehaviour
{
    public GameObject bulletPrefab; // Een GameObject met de naam bulletPrefab, wordt als kogel gebruikt
    public GameObject lightPrefab; // Een GameObject met de naam lightPrefab, wordt als schotlicht gebruikt
    public int rotatespeed = 120, speed = 5; // Rotatespeed en Movespeed
    public float fireinterval = 1, timestamp; // Float voor Fireinterval (niet gebruikt) en Timestamp
    public string type;
    private bool moveup, movedown, moveleft, moveright; // Of de knoppen in gebruik zijn
    [SyncVar] // Sync deze var naar alle spelers
    public string PlayerName = "";
    [SyncVar] // Sync deze var naar alle spelers
    public int score;
    public override void OnStartLocalPlayer() //Dit wordt uitgeroepen zodra de Prefab van de lokale speler spawnt
    {
        // Hele on-nette manier om de android knoppen hun triggers te geven (on push en on release)
        EventTrigger upButton = GuiController.singleton.upButton.GetComponent<EventTrigger>();
        EventTrigger leftButton = GuiController.singleton.leftButton.GetComponent<EventTrigger>();
        EventTrigger rightButton = GuiController.singleton.rightButton.GetComponent<EventTrigger>();
        EventTrigger downButton = GuiController.singleton.downButton.GetComponent<EventTrigger>();
        EventTrigger fireButton = GuiController.singleton.fireButton.GetComponent<EventTrigger>();
        EventTrigger.Entry entryup = new EventTrigger.Entry();
        EventTrigger.Entry entryupstop = new EventTrigger.Entry();
        EventTrigger.Entry entryleft = new EventTrigger.Entry();
        EventTrigger.Entry entryleftstop = new EventTrigger.Entry();
        EventTrigger.Entry entryright = new EventTrigger.Entry();
        EventTrigger.Entry entryrightstop = new EventTrigger.Entry();
        EventTrigger.Entry entrydown = new EventTrigger.Entry();
        EventTrigger.Entry entrydownstop = new EventTrigger.Entry();
        EventTrigger.Entry entryfire = new EventTrigger.Entry();
        entryup.eventID = EventTriggerType.PointerEnter;
        entryupstop.eventID = EventTriggerType.PointerExit;
        entryleft.eventID = EventTriggerType.PointerEnter;
        entryleftstop.eventID = EventTriggerType.PointerExit;
        entryright.eventID = EventTriggerType.PointerEnter;
        entryrightstop.eventID = EventTriggerType.PointerExit;
        entrydown.eventID = EventTriggerType.PointerEnter;
        entrydownstop.eventID = EventTriggerType.PointerExit;
        entryfire.eventID = EventTriggerType.PointerClick ;
        entryup.callback.AddListener((eventData) => { MoveUp(); });
        entryupstop.callback.AddListener((eventData) => { MoveUpStop(); });
        entryleft.callback.AddListener((eventData) => { MoveLeft(); });
        entryleftstop.callback.AddListener((eventData) => { MoveLeftStop(); });
        entryright.callback.AddListener((eventData) => { MoveRight(); });
        entryrightstop.callback.AddListener((eventData) => { MoveRightStop(); });
        entrydown.callback.AddListener((eventData) => { MoveDown(); });
        entrydownstop.callback.AddListener((eventData) => { MoveDownStop(); });
        entryfire.callback.AddListener((eventData) => { CmdFireMobile(); });
        upButton.triggers.Add(entryup);
        upButton.triggers.Add(entryupstop);
        leftButton.triggers.Add(entryleft);
        leftButton.triggers.Add(entryleftstop);
        rightButton.triggers.Add(entryright);
        rightButton.triggers.Add(entryrightstop);
        downButton.triggers.Add(entrydown);
        downButton.triggers.Add(entrydownstop);
        fireButton.triggers.Add(entryfire);
        Debug.Log(SystemInfo.deviceType); // Geeft een debug log in de Unity Console met de deviceType

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            /*
            if (SystemInfo.deviceType == DeviceType.Handheld)
            {
              //  r.material.color = Color.red;
            }
            if (SystemInfo.deviceType == DeviceType.Desktop)
            {
              //  r.material.color = Color.green;
            }
            */
        }
        Camera.main.GetComponent<CameraFollow>().setTarget(gameObject.transform); // Pak de script "CameraFollow" van de Camera en zet de Transform van de speler gameObject als target
    }

    [Command] //Wordt alleen op de server uitgevoerd
    void CmdFireMobile() // Tijdelijke code om de android fire knop een delay te geven
    {
        if (timestamp <= Time.time) // Als de tijd onder de huidige tijd is
        {
            timestamp = Time.time + 1; // timestamp is tijd + 1 sec
            CmdFire(); //Schiet
        }
    }
    [Command] //Command wordt op de server gedraaid
    void CmdFire()
    {
        var bullet = (GameObject)Instantiate(
             bulletPrefab,
             transform.position - transform.forward - transform.forward ,
             Quaternion.identity); // Maak een instantie van de bulletprefab voor de tank
        bullet.GetComponent<Rigidbody>().velocity = -transform.forward * 10; // De velocity van de Rigidbody van de tank (doet alle physics) is forward * 10 (10)
        bullet.GetComponent<Rigidbody>().transform.rotation = Quaternion.LookRotation(bullet.GetComponent<Rigidbody>().velocity); // Roteer de bullet naar de richting waar het geschoten wordt

        var light = (GameObject)Instantiate(
            lightPrefab,
            transform.position - transform.forward,
            Quaternion.identity); // Maak een instantie aan van de lightPrefab

        // spawn de kogel op elke client
        NetworkServer.Spawn(bullet); // Spawn
        NetworkServer.Spawn(light); // Spawn
        Destroy(bullet, 2.0f); // Destroy na 2 seconden
        Destroy(light, 0.1f); // Destroy na 0.1 seconde
    }

    void Update() // Wordt elke frame aangeroepen
    {
        var transAmount = speed * Time.deltaTime; // deltaTime zorgt ervoor dat de speed hetzelfde is op elke framerate
        var rotateAmount = rotatespeed * Time.deltaTime; // deltaTime zorgt ervoor dat de rotatespeed hetzelfde is op elke framerate
        if (!isLocalPlayer) // Als het niet om de localplayer gaat, doe niets
            return;
        // Axes kun je vinden onder Edit > Project Settings > Input
        if (Input.GetAxisRaw("Vertical") > 0 || moveup) // Als axis Vertical > 0 is of moveUp true is
        {
            transform.Translate(0, 0, -transAmount); // Ga naar voren
        }
        if (Input.GetAxisRaw("Vertical") < 0 || movedown) // Als axis Vertical < 0 is of moveDown true is
        {
            transform.Translate(0, 0, transAmount); // Ga naar achteren
        }
        if (Input.GetAxisRaw("Horizontal") < 0 || moveleft) // Als axis Horizontal < 0 is of moveLeft true is
        {
            transform.Rotate(0, -rotateAmount, 0); // Roteer naar links
        }
        if (Input.GetAxisRaw("Horizontal") > 0 || moveright) // Als axis Horizontal > 0 is of moveRight  true is
        {
            transform.Rotate(0, rotateAmount, 0); // Roteer naar rechts
        }
        if (Input.GetButton("Fire1") && timestamp <= Time.time) // Als er op Fire1 gedrukt wordt en de timestamp onder de huidige tijd is
        {
            timestamp = Time.time + 1; // timestamp is tijd + 1 sec
            CmdFire();
        }
    }
    public void MoveUp()
    {
        moveup = true;
    }
    public void MoveLeft()
    {
        moveleft = true;
    }
    public void MoveRight()
    {
        moveright = true;
    }
    public void MoveDown()
    {
        movedown = true;
    }
    public void MoveUpStop()
    {
        moveup = false;
    }
    public void MoveLeftStop()
    {
        moveleft = false;
    }
    public void MoveRightStop()
    {
        moveright = false;
    }
    public void MoveDownStop()
    {
        movedown = false;
    }
}