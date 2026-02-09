using Game.Client.App;
using UnityEngine;

public class GameBehaviour : MonoBehaviour
{
    private GameComposition _composition;

    private void Start()
    {
        _composition = new GameComposition();
    }

    // Update is called once per frame
    private void Update()
    {
        _composition?.Tick(Time.deltaTime);
    }
}
