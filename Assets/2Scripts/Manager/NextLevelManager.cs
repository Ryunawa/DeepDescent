using _2Scripts.ProceduralGeneration;

namespace _2Scripts.Manager
{
    public class NextLevelManager : Singleton<NextLevelManager>
    {
        public void GenerateNewDungeon()
        {
           // TODO: Show loading screen for players while dungeon generate (show loading screen from scene manager)
           SceneManager.instance.ActivateLoadingScreen();
           
           // TODO: Remove All the previous generated room (props too)
           Destroy(LevelGenerator.instance.generatedDungeonParent1);
           
           // TODO: Generate dungeon with the given seed
           LevelGenerator.instance.StartGeneration();
        }
    }
}