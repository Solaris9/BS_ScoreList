using ScoreList.UI;
using ScoreList.Utils;
using Zenject;

namespace ScoreList.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<LevelSelectionUtils>().AsSingle().NonLazy();
        
            Container.Bind<FilterViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ScoreViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<DetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ConfigViewController>().FromNewComponentAsViewController().AsSingle();
            
            Container.Bind<IInitializable>().To<ScoreListCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
        }
    }
}