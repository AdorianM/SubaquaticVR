using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void Explore()
    {
        SceneManager.LoadScene("Application");
    }

    public void MarchingCubes()
    {
        SceneManager.LoadScene("Terrain");
    }

    public void Boids()
    {
        SceneManager.LoadScene("Boids");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
