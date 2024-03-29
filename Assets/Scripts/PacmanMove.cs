﻿using System;
using System.Collections;
using Managers;
using UnityEngine;
using UX_Scripts;


public class PacmanMove : MonoBehaviour
{
    public float speed = 0.4f;
    Vector2 _dest = Vector2.zero;
    Vector2 _dir = Vector2.zero;
    Vector2 _nextDir = Vector2.zero;

    [Serializable]
    public class PointSprites
    {
        public GameObject[] pointSprites;
    }

    public PointSprites points;

    public static int killstreak = 0;

    //script from Manager folder
    private GameGUINavigation GUINav;
    private GameManager GM;
    private ScoreManager SM;

    private bool _deadPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        GM = GameObject.Find("Game Manager").GetComponent<GameManager>();
        SM = GameObject.Find("Game Manager").GetComponent<ScoreManager>();
        GUINav = GameObject.Find("UI Manager").GetComponent<GameGUINavigation>();
        _dest = transform.position;
    }

    // Update is called once per frame in a fixed time interval
    void FixedUpdate()
    {
        switch (GameManager.gameState)
        {
            case GameManager.GameState.Game:
                ReadInputAndMove();
                Animate();
                break;

            case GameManager.GameState.Dead:
                if (!_deadPlaying)
                    StartCoroutine("PlayDeadAnimation");
                break;
        }
    }

    IEnumerator PlayDeadAnimation()
    {
        _deadPlaying = true;
        GetComponent<Animator>().SetBool("Die", true);
        yield return new WaitForSeconds(1);
        GetComponent<Animator>().SetBool("Die", false);
        _deadPlaying = false;

        if (GameManager.lives <= 0)
        {
            Debug.Log("Threshold for the High Score: " + SM.LowestHigh());
            if (GameManager.score >= SM.LowestHigh())
                GUINav.GetScoresMenu();
            else
                GUINav.H_ShowGameOverScreen();
        }

        else 
            GM.ResetScene();
    }

    void Animate()
    {
    //Animation Parameters
        Vector2 dire = _dest - (Vector2)transform.position;
        GetComponent<Animator>().SetFloat("DirX", dire.x);
        GetComponent<Animator>().SetFloat("DirY", dire.y);
    }

    bool Valid(Vector2 direction) 
    {
        //Cast Line from 'next to Pac-Man' to 'Pac-Man'
        Vector2 pos = transform.position;
        direction += new Vector2(direction.x * 0.45f, direction.y * 0.45f);
        RaycastHit2D hit = Physics2D.Linecast(pos + direction, pos);
        return hit.collider.name == "pacdot" || (hit.collider == GetComponent<Collider2D>());
    }

    public void ResetDestination()
    {
        _dest = new Vector2(15f, 11f);
        GetComponent<Animator>().SetFloat("DirX", 1);
        GetComponent<Animator>().SetFloat("DirY", 0);
    }

    void ReadInputAndMove()
    {
        //move closer to destination
        Vector2 p = Vector2.MoveTowards(transform.position, _dest, speed);
        GetComponent<Rigidbody2D>().MovePosition(p);

        //get the next direction from keyboard
        if (Input.GetAxis("Horizontal") > 0) _nextDir = Vector2.right;
        if (Input.GetAxis("Horizontal") < 0) _nextDir = -Vector2.right;
        if (Input.GetAxis("Vertical") > 0) _nextDir = Vector2.up;
        if (Input.GetAxis("Vertical") < 0) _nextDir = -Vector2.up;

        // if pacman is in the center of a tile
        if (Vector2.Distance(_dest, transform.position) < 0.00001f)
        {
            if (Valid(_nextDir))
            {
                _dest = (Vector2)transform.position + _nextDir;
                _dir = _nextDir;
            }
            else //if direction is not valid
            {
                if (Valid(_dir)) // ant the previous direction is valid
                
                    _dest = (Vector2)transform.position + _dir; //continue on that direction

                    //otherwise, do nothing.
            }
        }
    }

        public Vector2 GetDir()
        {
            return _dir;
        }

        public void UpdateScore()
        {
            killstreak++;

            //limit kill-streak at 4
            if (killstreak > 4) killstreak = 4;

            Instantiate(points.pointSprites[killstreak - 1], transform.position, Quaternion.identity);
            GameManager.score += (int)Mathf.Pow(2, killstreak) * 100;
        }
}
